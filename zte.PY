import sys
import os
import subprocess

try:
    import customtkinter as ctk
except ModuleNotFoundError:
    sys.path.insert(0, "/usr/local/lib/python3.12/dist-packages")
    import customtkinter as ctk

from tkinter import messagebox, filedialog

from gerador import (
    PERFIS_TCONT, MAX_PON, MAX_ONU, MAX_VLAN,
    ConfiguracaoOLT, validar_int, validar_prefixo, gerar_script,
)
from perfis import CONFIG_FILE, salvar_perfil, carregar_perfil, carregar_perfil_auto

ctk.set_default_color_theme("blue")


def abrir_pasta(caminho: str) -> None:
    if sys.platform == "win32":
        os.startfile(caminho)
    elif sys.platform == "darwin":
        subprocess.Popen(["open", caminho])
    else:
        subprocess.Popen(["xdg-open", caminho])


class App(ctk.CTk):
    def __init__(self):
        super().__init__()
        self.title("Gerador OLT ZTE - Barra Connect")
        self.geometry("720x820")
        self.minsize(680, 680)
        self._ultima_pasta = os.getcwd()
        self._modo_escuro = False
        ctk.set_appearance_mode("light")
        self._criar_variaveis()
        self._criar_interface()
        self._aplicar_perfil_inicial()

    # ------------------------------------------------------------------ vars

    def _criar_variaveis(self):
        import tkinter as tk
        self.var_slot = tk.StringVar(value="1")
        self.var_card = tk.StringVar(value="3")
        self.var_pon_ini = tk.StringVar(value="1")
        self.var_pon_fim = tk.StringVar(value="1")
        self.var_vlan = tk.StringVar()
        self.var_onu_ini = tk.StringVar(value="1")
        self.var_onu_fim = tk.StringVar(value="128")
        self.var_perfil = tk.StringVar(value="1G")
        self.var_prefixo = tk.StringVar(value="automatico")
        self.var_servico2 = tk.BooleanVar(value=False)
        self.var_vlan2 = tk.StringVar()

    # ---------------------------------------------------------------- layout

    def _criar_interface(self):
        header = ctk.CTkFrame(self, fg_color="transparent")
        header.pack(fill="x", padx=16, pady=(14, 0))

        ctk.CTkLabel(
            header,
            text="Gerador OLT ZTE - Barra Connect",
            font=ctk.CTkFont(size=17, weight="bold"),
        ).pack(side="left")

        self.btn_tema = ctk.CTkButton(
            header, text="Modo Escuro", width=110,
            fg_color="transparent", border_width=1,
            text_color=("gray20", "gray80"),
            hover_color=("gray90", "gray30"),
            command=self._toggle_tema,
        )
        self.btn_tema.pack(side="right")

        ctk.CTkLabel(
            self, text="OLT ZTE TITAN",
            font=ctk.CTkFont(size=11), text_color="gray",
        ).pack(pady=(2, 8))

        self.tabview = ctk.CTkTabview(self)
        self.tabview.pack(fill="both", expand=True, padx=12, pady=(0, 4))

        self.tab_config = self.tabview.add("  Configuracao  ")
        self.tab_script = self.tabview.add("  Script Gerado  ")

        self._criar_tab_config()
        self._criar_tab_script()

        ctk.CTkLabel(
            self, text="By Rafael Caires & Lucas Alexandre",
            font=ctk.CTkFont(size=9), text_color="gray",
        ).pack(side="bottom", pady=(0, 6))

    def _toggle_tema(self):
        self._modo_escuro = not self._modo_escuro
        if self._modo_escuro:
            ctk.set_appearance_mode("dark")
            self.btn_tema.configure(text="Modo Claro")
        else:
            ctk.set_appearance_mode("light")
            self.btn_tema.configure(text="Modo Escuro")

    def _secao(self, parent, titulo: str) -> ctk.CTkFrame:
        frame = ctk.CTkFrame(parent, corner_radius=8)
        frame.pack(fill="x", padx=4, pady=(0, 8))
        frame.columnconfigure(1, weight=1)
        frame.columnconfigure(3, weight=1)
        ctk.CTkLabel(
            frame, text=titulo,
            font=ctk.CTkFont(size=12, weight="bold"),
        ).grid(row=0, column=0, columnspan=4, sticky="w", padx=14, pady=(10, 4))
        return frame

    def _campo(self, parent, label: str, var, row: int, col: int = 0,
               width: int = 110, values=None, columnspan: int = 1):
        r = row + 1
        ctk.CTkLabel(parent, text=label, font=ctk.CTkFont(size=11)).grid(
            row=r, column=col, sticky="w", padx=(14, 4), pady=6,
        )
        if values:
            w = ctk.CTkComboBox(parent, variable=var, values=values,
                                width=width, state="readonly")
        else:
            w = ctk.CTkEntry(parent, textvariable=var, width=width)
        w.grid(row=r, column=col + 1, columnspan=columnspan,
               sticky="w", padx=(4, 14), pady=6)
        return w

    def _criar_tab_config(self):
        p = self.tab_config

        # Barra de acao fixada na base, empacotada antes do scroll
        action_bar = ctk.CTkFrame(p, fg_color="transparent")
        action_bar.pack(fill="x", side="bottom", pady=(6, 2))

        self.lbl_status = ctk.CTkLabel(
            action_bar, text="",
            font=ctk.CTkFont(size=11),
            text_color=("green4", "lime green"),
        )
        self.lbl_status.pack(side="right", padx=14)

        ctk.CTkButton(
            action_bar,
            text="GERAR SCRIPT",
            font=ctk.CTkFont(size=14, weight="bold"),
            height=46, width=210,
            fg_color="#1a73e8", hover_color="#1558b0",
            command=self._gerar,
        ).pack(side="left", padx=14)

        # Conteudo com scroll
        scroll = ctk.CTkScrollableFrame(p, fg_color="transparent")
        scroll.pack(fill="both", expand=True, padx=0, pady=0)

        sec_olt = self._secao(scroll, "Enderecamento da OLT")
        self._campo(sec_olt, "Slot:", self.var_slot, 0, 0, width=100)
        self._campo(sec_olt, "Card:", self.var_card, 0, 2, width=100)
        self._campo(sec_olt, "PON Inicial:", self.var_pon_ini, 1, 0, width=100)
        self._campo(sec_olt, "PON Final:", self.var_pon_fim, 1, 2, width=100)

        sec_svc = self._secao(scroll, "Configuracao de Servico")
        self._campo(sec_svc, "VLAN Principal:", self.var_vlan, 0, 0, width=110)
        self._campo(sec_svc, "Perfil TCONT:", self.var_perfil, 0, 2,
                    width=160, values=PERFIS_TCONT)
        self._campo(sec_svc, "ONU Inicial:", self.var_onu_ini, 1, 0, width=100)
        self._campo(sec_svc, "ONU Final:", self.var_onu_fim, 1, 2, width=100)
        self._campo(sec_svc, "Prefixo Nome:", self.var_prefixo, 2, 0,
                    width=300, columnspan=3)

        sec_s2 = self._secao(scroll, "Servico Adicional (VoIP / IPTV)")
        ctk.CTkCheckBox(
            sec_s2, text="Habilitar segundo servico (gemport 2)",
            variable=self.var_servico2, command=self._toggle_servico2,
        ).grid(row=1, column=0, columnspan=4, sticky="w", padx=14, pady=6)
        ctk.CTkLabel(sec_s2, text="VLAN Servico 2:",
                     font=ctk.CTkFont(size=11)).grid(
            row=2, column=0, sticky="w", padx=(14, 4), pady=(0, 12),
        )
        self.entry_vlan2 = ctk.CTkEntry(
            sec_s2, textvariable=self.var_vlan2, width=110, state="disabled",
        )
        self.entry_vlan2.grid(row=2, column=1, sticky="w",
                              padx=(4, 14), pady=(0, 12))

        sec_perf = self._secao(scroll, "Perfil de Configuracao")
        ctk.CTkButton(
            sec_perf, text="Salvar Perfil", width=150,
            command=self._salvar_perfil,
        ).grid(row=1, column=0, columnspan=2, padx=(14, 5), pady=(6, 14), sticky="w")
        ctk.CTkButton(
            sec_perf, text="Carregar Perfil", width=150,
            fg_color="gray50", hover_color="#444",
            command=self._carregar_perfil,
        ).grid(row=1, column=2, columnspan=2, padx=(5, 14), pady=(6, 14), sticky="w")

    def _criar_tab_script(self):
        p = self.tab_script

        self.text_script = ctk.CTkTextbox(
            p, font=ctk.CTkFont(family="Courier", size=9),
            wrap="none", state="disabled",
        )
        self.text_script.pack(fill="both", expand=True, padx=10, pady=(10, 5))

        frame_btn = ctk.CTkFrame(p, fg_color="transparent")
        frame_btn.pack(pady=8)

        ctk.CTkButton(
            frame_btn, text="Copiar para Clipboard", width=190,
            fg_color="#28a745", hover_color="#1e7e34",
            command=self._copiar,
        ).pack(side="left", padx=5)
        ctk.CTkButton(
            frame_btn, text="Salvar Arquivo", width=150,
            command=self._salvar_arquivo,
        ).pack(side="left", padx=5)
        ctk.CTkButton(
            frame_btn, text="Abrir Pasta", width=130,
            fg_color="gray50", hover_color="#444",
            command=self._abrir_pasta_atual,
        ).pack(side="left", padx=5)

    # --------------------------------------------------------------- actions

    def _toggle_servico2(self):
        estado = "normal" if self.var_servico2.get() else "disabled"
        self.entry_vlan2.configure(state=estado)

    def _coletar_config(self) -> ConfiguracaoOLT:
        slot = validar_int(self.var_slot.get(), "Slot", 1, 99)
        card = validar_int(self.var_card.get(), "Card", 1, 99)
        pon_ini = validar_int(self.var_pon_ini.get(), "PON Inicial", 1, MAX_PON)
        pon_fim = validar_int(self.var_pon_fim.get(), "PON Final", 1, MAX_PON)
        vlan = validar_int(self.var_vlan.get(), "VLAN Principal", 1, MAX_VLAN)
        onu_ini = validar_int(self.var_onu_ini.get(), "ONU Inicial", 1, MAX_ONU)
        onu_fim = validar_int(self.var_onu_fim.get(), "ONU Final", 1, MAX_ONU)
        perfil = self.var_perfil.get()
        prefixo = validar_prefixo(self.var_prefixo.get())

        if pon_ini > pon_fim:
            raise ValueError("PON Inicial nao pode ser maior que PON Final.")
        if onu_ini > onu_fim:
            raise ValueError("ONU Inicial nao pode ser maior que ONU Final.")

        vlan2 = None
        if self.var_servico2.get():
            vlan2 = validar_int(self.var_vlan2.get(), "VLAN Servico 2", 1, MAX_VLAN)
            if vlan2 == vlan:
                raise ValueError("VLAN Servico 2 deve ser diferente da VLAN Principal.")

        return ConfiguracaoOLT(
            slot=slot, card=card,
            pon_ini=pon_ini, pon_fim=pon_fim,
            vlan=vlan, onu_ini=onu_ini, onu_fim=onu_fim,
            perfil=perfil, prefixo=prefixo, vlan2=vlan2,
        )

    def _gerar(self):
        self.lbl_status.configure(text="")
        try:
            cfg = self._coletar_config()
        except ValueError as e:
            messagebox.showerror("Erro de Validacao", str(e))
            return

        texto = gerar_script(cfg)

        self.text_script.configure(state="normal")
        self.text_script.delete("1.0", "end")
        self.text_script.insert("1.0", texto)
        self.text_script.configure(state="disabled")

        total = (cfg.pon_fim - cfg.pon_ini + 1) * (cfg.onu_fim - cfg.onu_ini + 1)
        self.lbl_status.configure(
            text=f"OK  {total} ONU(s) | PON {cfg.pon_ini}->{cfg.pon_fim} | VLAN {cfg.vlan}"
        )
        self.tabview.set("  Script Gerado  ")

    def _conteudo_script(self) -> str:
        return self.text_script.get("1.0", "end").strip()

    def _copiar(self):
        conteudo = self._conteudo_script()
        if not conteudo:
            messagebox.showwarning("Aviso", "Nenhum script gerado ainda.")
            return
        self.clipboard_clear()
        self.clipboard_append(conteudo)
        messagebox.showinfo("Copiado", "Script copiado para a area de transferencia!")

    def _salvar_arquivo(self):
        conteudo = self._conteudo_script()
        if not conteudo:
            messagebox.showwarning("Aviso", "Nenhum script gerado ainda.")
            return
        nome_sugerido = (
            f"script_pon{self.var_pon_ini.get()}-{self.var_pon_fim.get()}"
            f"_vlan{self.var_vlan.get()}.txt"
        )
        caminho = filedialog.asksaveasfilename(
            defaultextension=".txt",
            initialfile=nome_sugerido,
            filetypes=[("Arquivo de Texto", "*.txt"), ("Todos os Arquivos", "*.*")],
        )
        if not caminho:
            return
        try:
            with open(caminho, "w", encoding="utf-8") as f:
                f.write(conteudo)
            self._ultima_pasta = os.path.dirname(caminho)
            messagebox.showinfo("Salvo", f"Arquivo salvo:\n{caminho}")
        except PermissionError:
            messagebox.showerror("Erro de Permissao",
                                 f"Sem permissao para salvar em:\n{caminho}")
        except OSError as e:
            messagebox.showerror("Erro ao Salvar", f"Erro ao salvar o arquivo:\n{e}")

    def _abrir_pasta_atual(self):
        abrir_pasta(self._ultima_pasta)

    # --------------------------------------------------------------- perfis

    def _dados_perfil(self) -> dict:
        return {
            "slot": self.var_slot.get(),
            "card": self.var_card.get(),
            "pon_ini": self.var_pon_ini.get(),
            "pon_fim": self.var_pon_fim.get(),
            "vlan": self.var_vlan.get(),
            "onu_ini": self.var_onu_ini.get(),
            "onu_fim": self.var_onu_fim.get(),
            "perfil": self.var_perfil.get(),
            "prefixo": self.var_prefixo.get(),
            "servico2": self.var_servico2.get(),
            "vlan2": self.var_vlan2.get(),
        }

    def _aplicar_perfil(self, dados: dict):
        self.var_slot.set(dados.get("slot", "1"))
        self.var_card.set(dados.get("card", "3"))
        self.var_pon_ini.set(dados.get("pon_ini", "1"))
        self.var_pon_fim.set(dados.get("pon_fim", "1"))
        self.var_vlan.set(dados.get("vlan", ""))
        self.var_onu_ini.set(dados.get("onu_ini", "1"))
        self.var_onu_fim.set(dados.get("onu_fim", "128"))
        self.var_perfil.set(dados.get("perfil", "1G"))
        self.var_prefixo.set(dados.get("prefixo", "automatico"))
        self.var_servico2.set(dados.get("servico2", False))
        self.var_vlan2.set(dados.get("vlan2", ""))
        self._toggle_servico2()

    def _aplicar_perfil_inicial(self):
        dados = carregar_perfil_auto()
        if dados:
            self._aplicar_perfil(dados)

    def _salvar_perfil(self):
        caminho = filedialog.asksaveasfilename(
            defaultextension=".json",
            initialfile=CONFIG_FILE,
            filetypes=[("Perfil JSON", "*.json"), ("Todos os Arquivos", "*.*")],
        )
        if not caminho:
            return
        try:
            salvar_perfil(self._dados_perfil(), caminho)
            messagebox.showinfo("Salvo", f"Perfil salvo em:\n{caminho}")
        except PermissionError:
            messagebox.showerror("Erro de Permissao",
                                 f"Sem permissao para salvar em:\n{caminho}")
        except OSError as e:
            messagebox.showerror("Erro ao Salvar", f"Erro ao salvar perfil:\n{e}")

    def _carregar_perfil(self):
        caminho = filedialog.askopenfilename(
            filetypes=[("Perfil JSON", "*.json"), ("Todos os Arquivos", "*.*")],
        )
        if not caminho:
            return
        try:
            self._aplicar_perfil(carregar_perfil(caminho))
            messagebox.showinfo("Carregado", "Perfil carregado com sucesso!")
        except ValueError as e:
            messagebox.showerror("Erro de Formato", str(e))
        except OSError as e:
            messagebox.showerror("Erro ao Carregar", f"Erro ao carregar perfil:\n{e}")


if __name__ == "__main__":
    app = App()
    app.mainloop()
