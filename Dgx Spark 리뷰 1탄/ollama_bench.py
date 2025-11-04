#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Ollama API GUI 벤치 툴 (수직 스크롤만, 가로 스크롤 제거)
- IP:PORT 입력 → [핑]으로 연결 확인 및 모델 목록 로드
- 모델 선택, num_predict, 프롬프트 입력 후 [Run]
- 생성 결과가 실시간으로 아래 텍스트 창에 스트리밍 표시
- 최종적으로 prefill / decode 토큰/초(TPS) + 토큰 수 + TTFT + 총 시간 표시
"""

import json
import threading
import time
import tkinter as tk
from tkinter import ttk, messagebox
from tkinter.scrolledtext import ScrolledText
from urllib.parse import urlparse

import requests


# -------------------------- 공용 유틸 ---------------------------
def _norm_base_url(raw: str) -> str:
    raw = (raw or "").strip()
    if not raw:
        return "http://127.0.0.1:11434"
    if not raw.startswith("http://") and not raw.startswith("https://"):
        raw = "http://" + raw
    return raw.rstrip("/")


class ScrollableFrame(ttk.Frame):

    def __init__(self, parent):
        super().__init__(parent)

        self.canvas = tk.Canvas(self, highlightthickness=0)
        self.vbar = ttk.Scrollbar(self, orient="vertical", command=self.canvas.yview)
        self.canvas.configure(yscrollcommand=self.vbar.set)

        # 실제 내용이 들어갈 프레임
        self.inner = ttk.Frame(self.canvas)
        self.inner_id = self.canvas.create_window((0, 0), window=self.inner, anchor="nw")

        # 내용/캔버스 크기 변화 시 스크롤 영역 동기화 + 내부 폭을 캔버스 폭에 맞춤
        self.inner.bind("<Configure>", self._sync_scrollregion)
        self.canvas.bind("<Configure>", self._on_canvas_configure)

        # 스크롤 동작(세로만)
        self.canvas.bind_all("<MouseWheel>", self._on_mousewheel)      # Win/macOS
        # Linux 휠 이벤트
        self.canvas.bind_all("<Button-4>", lambda e: self.canvas.yview_scroll(-1, "units"))
        self.canvas.bind_all("<Button-5>", lambda e: self.canvas.yview_scroll(1, "units"))

        # 배치(Grid)
        self.canvas.grid(row=0, column=0, sticky="nsew")
        self.vbar.grid(row=0, column=1, sticky="ns")
        self.grid_rowconfigure(0, weight=1)
        self.grid_columnconfigure(0, weight=1)

    def _sync_scrollregion(self, _):
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))

    def _on_canvas_configure(self, e):
        # 내부 프레임 폭을 캔버스 폭에 고정 → 가로 스크롤 없음
        self.canvas.itemconfigure(self.inner_id, width=e.width)

    def _on_mousewheel(self, event):
        if event.delta:
            self.canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")


# -------------------------- 메인 GUI ----------------------------
class OllamaBenchGUI(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Ollama GUI Bench – Prefill/Decode TPS")
        self.geometry("980x720")
        self.minsize(720, 520)

        self._lock = threading.Lock()
        self._run_thread = None
        self._stop_flag = False

        self._build_widgets()
        self._build_menu()  # 배율(줌) 조절 옵션

    # ---------------- UI -----------------
    def _build_widgets(self):
        # HiDPI에서 스케일이 과/소배율일 때 조정하고 싶다면 아래 값 변경(예: 1.25)
        try:
            self.tk.call('tk', 'scaling', 1.0)
        except Exception:
            pass

        # 전체를 스크롤 컨테이너로 감싸서 작은 창에서도 스크롤로 접근 가능(세로만)
        scroll = ScrollableFrame(self)
        scroll.pack(fill="both", expand=True)
        parent = scroll.inner

        # 상단 연결 프레임
        conn_frame = ttk.LabelFrame(parent, text="서버 연결")
        conn_frame.pack(fill="x", padx=10, pady=8)

        ttk.Label(conn_frame, text="IP:PORT 또는 URL").grid(row=0, column=0, sticky="w", padx=8, pady=6)
        self.base_entry = ttk.Entry(conn_frame)
        self.base_entry.insert(0, "http://127.0.0.1:11434")
        self.base_entry.grid(row=0, column=1, sticky="ew", padx=8, pady=6)
        conn_frame.columnconfigure(1, weight=1)

        self.ping_btn = ttk.Button(conn_frame, text="핑", command=self.on_ping)
        self.ping_btn.grid(row=0, column=2, sticky="e", padx=8, pady=6)

        self.conn_status = ttk.Label(conn_frame, text="상태: 대기중", foreground="#444")
        self.conn_status.grid(row=0, column=3, sticky="e", padx=8, pady=6)

        # 옵션 프레임
        opt_frame = ttk.LabelFrame(parent, text="테스트 옵션")
        opt_frame.pack(fill="x", padx=10, pady=8)

        ttk.Label(opt_frame, text="모델").grid(row=0, column=0, sticky="w", padx=8, pady=6)
        self.model_combo = ttk.Combobox(opt_frame, state="disabled")
        self.model_combo.grid(row=0, column=1, sticky="ew", padx=8, pady=6)

        ttk.Label(opt_frame, text="num_predict").grid(row=0, column=2, sticky="w", padx=8, pady=6)
        self.num_predict_entry = ttk.Entry(opt_frame, width=10)
        self.num_predict_entry.insert(0, "128")
        self.num_predict_entry.grid(row=0, column=3, sticky="w", padx=8, pady=6)

        self.run_btn = ttk.Button(opt_frame, text="Run", command=self.on_run, state="disabled")
        self.run_btn.grid(row=0, column=4, sticky="e", padx=8, pady=6)

        self.stop_btn = ttk.Button(opt_frame, text="중지", command=self.on_stop, state="disabled")
        self.stop_btn.grid(row=0, column=5, sticky="e", padx=8, pady=6)

        opt_frame.columnconfigure(1, weight=1)

        # 프롬프트 입력 (세로 스크롤)
        prompt_frame = ttk.LabelFrame(parent, text="프롬프트")
        prompt_frame.pack(fill="both", expand=False, padx=10, pady=8)
        self.prompt_text = ScrolledText(prompt_frame, height=8, wrap="word")
        self.prompt_text.insert("1.0", "여기에 프롬프트를 입력하세요…")
        self.prompt_text.pack(fill="both", expand=True, padx=8, pady=8)

        # 결과 스트리밍 출력 (가로 스크롤 제거, 자동 줄바꿈)
        out_frame = ttk.LabelFrame(parent, text="실시간 출력")
        out_frame.pack(fill="both", expand=True, padx=10, pady=8)
        self.output_text = ScrolledText(out_frame, height=18, wrap="word")
        self.output_text.configure(state="disabled")
        self.output_text.pack(fill="both", expand=True, padx=8, pady=8)

        # 메트릭스
        metrics = ttk.LabelFrame(parent, text="측정 지표")
        metrics.pack(fill="x", padx=10, pady=8)

        self.prefill_tps_var = tk.StringVar(value="-")
        self.decode_tps_var = tk.StringVar(value="-")
        self.prompt_tok_var = tk.StringVar(value="-")
        self.decode_tok_var = tk.StringVar(value="-")
        self.ttft_var = tk.StringVar(value="-")
        self.total_time_var = tk.StringVar(value="-")

        self._metric_row(metrics, 0, "Prefill TPS", self.prefill_tps_var)
        self._metric_row(metrics, 0, "Decode TPS", self.decode_tps_var, col=2)
        self._metric_row(metrics, 1, "Prompt Tokens", self.prompt_tok_var)
        self._metric_row(metrics, 1, "Decode Tokens", self.decode_tok_var, col=2)
        self._metric_row(metrics, 2, "TTFT(초)", self.ttft_var)
        self._metric_row(metrics, 2, "Total Time(초)", self.total_time_var, col=2)

        for c in range(4):
            metrics.columnconfigure(c, weight=1)

    def _build_menu(self):
        menubar = tk.Menu(self)
        view = tk.Menu(menubar, tearoff=0)

        def get_scale():
            try:
                return float(self.tk.call('tk', 'scaling'))
            except Exception:
                return 1.0

        def set_scale(val: float):
            try:
                self.tk.call('tk', 'scaling', val)
            except Exception:
                pass

        view.add_command(label="Zoom In (Ctrl+=)", command=lambda: set_scale(get_scale() + 0.1))
        view.add_command(label="Zoom Out (Ctrl+-)", command=lambda: set_scale(max(0.5, get_scale() - 0.1)))
        view.add_command(label="Reset Zoom (Ctrl+0)", command=lambda: set_scale(1.0))
        menubar.add_cascade(label="보기", menu=view)
        self.config(menu=menubar)

        # 단축키
        self.bind_all("<Control-plus>", lambda e: set_scale(get_scale() + 0.1))
        self.bind_all("<Control-equal>", lambda e: set_scale(get_scale() + 0.1))
        self.bind_all("<Control-minus>", lambda e: set_scale(max(0.5, get_scale() - 0.1)))
        self.bind_all("<Control-0>", lambda e: set_scale(1.0))

    def _metric_row(self, parent, row, label, var, col=0):
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="w", padx=8, pady=6)
        ttk.Label(parent, textvariable=var, foreground="#0a6").grid(row=row, column=col+1, sticky="w", padx=8, pady=6)

    # --------------- helpers ---------------
    def set_status(self, text, ok=False):
        color = "#0a6" if ok else "#a60"
        self.conn_status.configure(text=f"상태: {text}", foreground=color)

    def append_output(self, text):
        self.output_text.configure(state="normal")
        self.output_text.insert("end", text)
        self.output_text.see("end")
        self.output_text.configure(state="disabled")

    def clear_output(self):
        self.output_text.configure(state="normal")
        self.output_text.delete("1.0", "end")
        self.output_text.configure(state="disabled")

    # --------------- actions ---------------
    def on_ping(self):
        base = _norm_base_url(self.base_entry.get())
        try:
            _ = urlparse(base)
        except Exception:
            messagebox.showerror("오류", "유효하지 않은 URL 형식입니다.")
            return

        self.set_status("핑 중…")
        self.ping_btn.configure(state="disabled")

        def worker():
            try:
                t0 = time.perf_counter()
                url = f"{base}/api/tags"
                r = requests.get(url, timeout=6)
                r.raise_for_status()
                data = r.json()
                latency_ms = (time.perf_counter() - t0) * 1000

                models = []
                if isinstance(data, dict) and "models" in data and isinstance(data["models"], list):
                    for m in data["models"]:
                        name = m.get("name")
                        if name:
                            models.append(name)
                elif isinstance(data, list):  # 일부 버전 호환
                    for m in data:
                        name = m.get("name") if isinstance(m, dict) else None
                        if name:
                            models.append(name)

                models = sorted(set(models))

                def ui_ok():
                    self.model_combo.configure(state="readonly", values=models)
                    if models:
                        self.model_combo.current(0)
                    self.run_btn.configure(state="normal")
                    self.stop_btn.configure(state="disabled")
                    self.set_status(f"연결 성공 – {latency_ms:.1f} ms, 모델 {len(models)}개", ok=True)
                self.after(0, ui_ok)
            except Exception as e:
                def ui_err():
                    self.set_status(f"실패: {e}")
                    self.model_combo.configure(state="disabled", values=[])
                    self.run_btn.configure(state="disabled")
                self.after(0, ui_err)
            finally:
                def ui_final():
                    self.ping_btn.configure(state="normal")
                self.after(0, ui_final)

        threading.Thread(target=worker, daemon=True).start()

    def on_stop(self):
        self._stop_flag = True
        self.stop_btn.configure(state="disabled")

    def on_run(self):
        base = _norm_base_url(self.base_entry.get())
        model = (self.model_combo.get() or "").strip()
        if not model:
            messagebox.showwarning("확인", "모델을 선택하세요.")
            return

        try:
            num_predict = int(self.num_predict_entry.get())
            if num_predict <= 0:
                raise ValueError
        except Exception:
            messagebox.showwarning("확인", "num_predict는 양의 정수여야 합니다.")
            return

        prompt = self.prompt_text.get("1.0", "end").strip()
        if not prompt:
            messagebox.showwarning("확인", "프롬프트를 입력하세요.")
            return

        if self._run_thread and self._run_thread.is_alive():
            messagebox.showinfo("안내", "이미 실행 중입니다. 중지 후 다시 실행하세요.")
            return

        self.clear_output()
        self.prefill_tps_var.set("-")
        self.decode_tps_var.set("-")
        self.prompt_tok_var.set("-")
        self.decode_tok_var.set("-")
        self.ttft_var.set("-")
        self.total_time_var.set("-")

        self._stop_flag = False
        self.run_btn.configure(state="disabled")
        self.stop_btn.configure(state="normal")
        self.set_status("실행 중…", ok=True)

        self._run_thread = threading.Thread(
            target=self._run_worker,
            args=(base, model, num_predict, prompt),
            daemon=True,
        )
        self._run_thread.start()

    # -------------- networking --------------
    def _run_worker(self, base, model, num_predict, prompt):
        url = f"{base}/api/generate"
        headers = {"Content-Type": "application/json"}
        payload = {
            "model": model,
            "prompt": prompt,
            "stream": True,
            "options": {
                "num_predict": num_predict
            },
        }

        t_start = time.perf_counter()
        t_first_token = None

        # 최종 통계 (Ollama 제공 값)
        prompt_eval_count = None
        prompt_eval_duration = None
        eval_count = None
        eval_duration = None
        total_duration = None

        try:
            with requests.post(url, headers=headers, data=json.dumps(payload), stream=True, timeout=(6, None)) as r:
                r.raise_for_status()
                for raw in r.iter_lines(decode_unicode=True):
                    if self._stop_flag:
                        break
                    if not raw:
                        continue
                    try:
                        item = json.loads(raw)
                    except json.JSONDecodeError:
                        continue

                    if "error" in item:
                        raise RuntimeError(item.get("error"))

                    # 스트리밍 토큰
                    chunk = item.get("response", "")
                    if chunk:
                        if t_first_token is None:
                            t_first_token = time.perf_counter()
                        self.after(0, self.append_output, chunk)

                    # 완료 시점의 통계(마지막 청크)
                    if item.get("done"):
                        prompt_eval_count = item.get("prompt_eval_count")
                        prompt_eval_duration = item.get("prompt_eval_duration")  # ns
                        eval_count = item.get("eval_count")
                        eval_duration = item.get("eval_duration")  # ns
                        total_duration = item.get("total_duration")  # ns
                        break

            # 메트릭 계산
            t_end = time.perf_counter()
            ttft = (t_first_token - t_start) if t_first_token else None
            total_sec = (total_duration / 1e9) if total_duration else (t_end - t_start)

            prefill_tps = None
            decode_tps = None
            if prompt_eval_count and prompt_eval_duration and prompt_eval_duration > 0:
                prefill_tps = float(prompt_eval_count) / (prompt_eval_duration / 1e9)
            if eval_count and eval_duration and eval_duration > 0:
                decode_tps = float(eval_count) / (eval_duration / 1e9)

            def ui_done():
                if ttft is not None:
                    self.ttft_var.set(f"{ttft:.3f}")
                else:
                    self.ttft_var.set("-")
                self.total_time_var.set(f"{total_sec:.3f}")
                self.prompt_tok_var.set(str(prompt_eval_count) if prompt_eval_count is not None else "-")
                self.decode_tok_var.set(str(eval_count) if eval_count is not None else "-")
                self.prefill_tps_var.set(f"{prefill_tps:.2f}" if prefill_tps is not None else "-")
                self.decode_tps_var.set(f"{decode_tps:.2f}" if decode_tps is not None else "-")
                self.set_status("완료", ok=True)
                self.run_btn.configure(state="normal")
                self.stop_btn.configure(state="disabled")
            self.after(0, ui_done)

        except Exception as e:
            def ui_err():
                self.set_status(f"오류: {e}")
                messagebox.showerror("실행 오류", str(e))
                self.run_btn.configure(state="normal")
                self.stop_btn.configure(state="disabled")
            self.after(0, ui_err)


if __name__ == "__main__":
    app = OllamaBenchGUI()
    app.mainloop()

