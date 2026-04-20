import tkinter as tk
from tkinter import ttk, messagebox, filedialog
import os
import sys
import json
import subprocess

PERFIS_TCONT = ["100M", "200M", "500M", "1G", "2G", "1GPON"]
CONFIG_FILE = "perfil_config.json"

COR_FUNDO = "#f0f0f0"
COR_BOTAO = "#0056B8"
COR_BOTAO2 = "#28a745"
COR_BOTAO3 = "#6c757d"
COR_INFO = "#17a2b8"


def abrir_pasta(caminho):
    if sys.platform == "win32":
        os.startfile(caminho)
    elif sys.platform == "darwin":
        subprocess.Popen(["open", caminho])
    else:
        subprocess.Popen(["xdg-open", caminho])


def validar_int(valor, nome, minimo, maximo):
    valor = valor.strip()
    if not valor.isdigit():
        raise ValueError(f"'{nome}' deve ser um número inteiro.")
    n = int(valor)
    if not (minimo <= n <= maximo):
        raise ValueError(f"'{nome}' deve estar entre {minimo} e {maximo}.")
    return n


def gerar_texto_script(slot, card, pon_ini, pon_fim, vlan,
                       onu_ini, onu_fim, perfil, prefixo, vlan2=None):
    sep = "! " + "#" * 56
    linhas = []
    for pon in range(pon_ini, pon_fim + 1):
        linhas += [
            sep,
            f"! ### INICIO CONFIGURACAO PON {pon} / VLAN {vlan} ###",
            sep,
            "",
        ]
        for i in range(onu_ini, onu_fim + 1):
            linhas.append(f"! Configuracao ONU {i}")
            linhas.append(f"interface gpon_onu-{slot}/{card}/{pon}:{i}")
            linhas.append(f"name {prefixo}_{pon}_{i}")
            linhas.append(f"description {prefixo}_{pon}_{i}")
            linhas.append(f"tcont 4 profile {perfil}")
            linhas.append(f"gemport 1 name GEM1 tcont 4")
            if vlan2:
                linhas.append(f"gemport 2 name GEM2 tcont 4")
            linhas.append("exit")
            linhas.append(f"interface vport-{slot}/{card}/{pon}.{i}:1")
            linhas.append(f"service-port 1 user-vlan {vlan} vlan {vlan}")
            if vlan2:
                linhas.append(f"service-port 2 user-vlan {vlan2} vlan {vlan2}")
            linhas.append("exit")
            linhas.append(f"pon-onu-mng gpon_onu-{slot}/{card}/{pon}:{i}")
            linhas.append(f"service 1 gemport 1 vlan {vlan}")
            linhas.append(f"vlan port eth_0/1 mode tag vlan {vlan}")
            if vlan2:
                linhas.append(f"service 2 gemport 2 vlan {vlan2}")
                linhas.append(f"vlan port eth_0/2 mode tag vlan {vlan2}")
            linhas.append("exit")
            linhas.append("")
        linhas += [
            sep,
            f"! ### FIM CONFIGURACAO PON {pon} / VLAN {vlan} ###",
            sep,
            "",
        ]
    return "\n".join(linhas)


class App(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("Gerador OLT ZTE - Barra Connect")
        self.geometry("640x720")
        self.resizable(True, True)
        self.configure(bg=COR_FUNDO)
        self._ultima_pasta = os.getcwd()
        self._criar_variaveis()
        self._criar_interface()
        self._carregar_perfil_auto()

    def _criar_variaveis(self):
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

    def _criar_secao(self, parent, titulo):
        frame = tk.LabelFrame(
            parent, text=titulo, font=("Arial", 10, "bold"),
            bg=COR_FUNDO, padx=10, pady=8
        )
        frame.pack(fill=tk.X, padx=10, pady=5)
        return frame

    def _campo(self, parent, label, var, row, col=0, width=10, values=None):
        tk.Label(parent, text=label, font=("Arial", 10), bg=COR_FUNDO).grid(
            row=row, column=col, sticky=tk.W, padx=5, pady=3
        )
        if values:
            w = ttk.Combobox(
                parent, textvariable=var, values=values,
                width=width - 2, state="readonly", font=("Arial", 10)
            )
        else:
            w = tk.Entry(parent, textvariable=var, width=width, font=("Arial", 10))
        w.grid(row=row, column=col + 1, sticky=tk.W, padx=5, pady=3)
        return w

    def _criar_interface(self):
        tk.Label(
            self, text="Gerador OLT ZTE — Barra Connect",
            font=("Arial", 14, "bold"), bg=COR_FUNDO
        ).pack(pady=(15, 2))
        tk.Label(
            self, text="OLT ZTE TITAN",
            font=("Arial", 10, "italic"), bg=COR_FUNDO, fg="#666"
        ).pack(pady=(0, 5))

        self.nb = ttk.Notebook(self)
        self.nb.pack(fill=tk.BOTH, expand=True, padx=10, pady=5)

        self.tab_config = tk.Frame(self.nb, bg=COR_FUNDO)
        self.tab_script = tk.Frame(self.nb, bg=COR_FUNDO)
        self.nb.add(self.tab_config, text="  Configuracao  ")
        self.nb.add(self.tab_script, text="  Script Gerado  ")

        self._criar_tab_config()
        self._criar_tab_script()

        tk.Label(
            self, text="By Rafael Caires & Lucas Alexandre",
            font=("Arial", 9), bg=COR_FUNDO, fg="gray"
        ).pack(side=tk.BOTTOM, pady=5)

    def _criar_tab_config(self):
        p = self.tab_config

        sec_olt = self._criar_secao(p, "Enderecamento da OLT")
        self._campo(sec_olt, "Slot:", self.var_slot, 0, 0, width=6)
        self._campo(sec_olt, "Card:", self.var_card, 0, 2, width=6)
        self._campo(sec_olt, "PON Inicial:", self.var_pon_ini, 1, 0, width=6)
        self._campo(sec_olt, "PON Final:", self.var_pon_fim, 1, 2, width=6)

        sec_svc = self._criar_secao(p, "Configuracao de Servico")
        self._campo(sec_svc, "VLAN Principal:", self.var_vlan, 0, 0, width=8)
        self._campo(sec_svc, "Perfil TCONT:", self.var_perfil, 0, 2,
                    width=12, values=PERFIS_TCONT)
        self._campo(sec_svc, "ONU Inicial:", self.var_onu_ini, 1, 0, width=6)
        self._campo(sec_svc, "ONU Final:", self.var_onu_fim, 1, 2, width=6)
        self._campo(sec_svc, "Prefixo Nome:", self.var_prefixo, 2, 0, width=22)

        sec_s2 = self._criar_secao(p, "Servico Adicional (VoIP / IPTV)")
        tk.Checkbutton(
            sec_s2, text="Habilitar segundo servico (gemport 2)",
            variable=self.var_servico2, font=("Arial", 10),
            bg=COR_FUNDO, command=self._toggle_servico2
        ).grid(row=0, column=0, columnspan=4, sticky=tk.W, padx=5)
        tk.Label(sec_s2, text="VLAN Servico 2:", font=("Arial", 10),
                 bg=COR_FUNDO).grid(row=1, column=0, sticky=tk.W, padx=5, pady=3)
        self.entry_vlan2 = tk.Entry(
            sec_s2, textvariable=self.var_vlan2,
            width=8, font=("Arial", 10), state=tk.DISABLED
        )
        self.entry_vlan2.grid(row=1, column=1, sticky=tk.W, padx=5, pady=3)

        sec_perf = self._criar_secao(p, "Perfil de Configuracao")
        tk.Button(
            sec_perf, text="Salvar Perfil", font=("Arial", 10),
            bg=COR_INFO, fg="white", command=self._salvar_perfil
        ).pack(side=tk.LEFT, padx=5)
        tk.Button(
            sec_perf, text="Carregar Perfil", font=("Arial", 10),
            bg=COR_BOTAO3, fg="white", command=self._carregar_perfil
        ).pack(side=tk.LEFT, padx=5)

        tk.Button(
            p, text="GERAR SCRIPT", font=("Arial", 13, "bold"),
            bg=COR_BOTAO, fg="white", height=2, width=22,
            command=self._gerar
        ).pack(pady=15)

    def _criar_tab_script(self):
        p = self.tab_script

        frame_txt = tk.Frame(p, bg=COR_FUNDO)
        frame_txt.pack(fill=tk.BOTH, expand=True, padx=10, pady=(10, 0))

        vscroll = tk.Scrollbar(frame_txt)
        vscroll.pack(side=tk.RIGHT, fill=tk.Y)

        self.text_script = tk.Text(
            frame_txt, font=("Courier", 9), wrap=tk.NONE,
            yscrollcommand=vscroll.set
        )
        self.text_script.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        vscroll.config(command=self.text_script.yview)

        hscroll = tk.Scrollbar(p, orient=tk.HORIZONTAL,
                               command=self.text_script.xview)
        hscroll.pack(fill=tk.X, padx=10)
        self.text_script.config(xscrollcommand=hscroll.set)

        frame_btn = tk.Frame(p, bg=COR_FUNDO)
        frame_btn.pack(pady=8)

        tk.Button(
            frame_btn, text="Copiar para Clipboard", font=("Arial", 10),
            bg=COR_BOTAO2, fg="white", command=self._copiar
        ).pack(side=tk.LEFT, padx=5)
        tk.Button(
            frame_btn, text="Salvar Arquivo", font=("Arial", 10),
            bg=COR_BOTAO, fg="white", command=self._salvar_arquivo
        ).pack(side=tk.LEFT, padx=5)
        tk.Button(
            frame_btn, text="Abrir Pasta", font=("Arial", 10),
            bg=COR_BOTAO3, fg="white", command=self._abrir_pasta_atual
        ).pack(side=tk.LEFT, padx=5)

    def _toggle_servico2(self):
        estado = tk.NORMAL if self.var_servico2.get() else tk.DISABLED
        self.entry_vlan2.config(state=estado)

    def _coletar_dados(self):
        slot = validar_int(self.var_slot.get(), "Slot", 1, 99)
        card = validar_int(self.var_card.get(), "Card", 1, 99)
        pon_ini = validar_int(self.var_pon_ini.get(), "PON Inicial", 1, 16)
        pon_fim = validar_int(self.var_pon_fim.get(), "PON Final", 1, 16)
        vlan = validar_int(self.var_vlan.get(), "VLAN Principal", 1, 4096)
        onu_ini = validar_int(self.var_onu_ini.get(), "ONU Inicial", 1, 128)
        onu_fim = validar_int(self.var_onu_fim.get(), "ONU Final", 1, 128)
        perfil = self.var_perfil.get()
        prefixo = self.var_prefixo.get().strip() or "automatico"

        if pon_ini > pon_fim:
            raise ValueError("PON Inicial nao pode ser maior que PON Final.")
        if onu_ini > onu_fim:
            raise ValueError("ONU Inicial nao pode ser maior que ONU Final.")

        vlan2 = None
        if self.var_servico2.get():
            vlan2 = validar_int(self.var_vlan2.get(), "VLAN Servico 2", 1, 4096)
            if vlan2 == vlan:
                raise ValueError("VLAN Servico 2 deve ser diferente da VLAN Principal.")

        return slot, card, pon_ini, pon_fim, vlan, onu_ini, onu_fim, perfil, prefixo, vlan2

    def _gerar(self):
        try:
            dados = self._coletar_dados()
        except ValueError as e:
            messagebox.showerror("Erro de Validacao", str(e))
            return

        slot, card, pon_ini, pon_fim, vlan, onu_ini, onu_fim, perfil, prefixo, vlan2 = dados
        texto = gerar_texto_script(
            slot, card, pon_ini, pon_fim, vlan,
            onu_ini, onu_fim, perfil, prefixo, vlan2
        )

        self.text_script.config(state=tk.NORMAL)
        self.text_script.delete("1.0", tk.END)
        self.text_script.insert("1.0", texto)
        self.text_script.config(state=tk.DISABLED)

        total = (pon_fim - pon_ini + 1) * (onu_fim - onu_ini + 1)
        messagebox.showinfo(
            "Script Gerado",
            f"Script gerado com sucesso!\n"
            f"PONs: {pon_ini} ate {pon_fim}  |  ONUs: {onu_ini} ate {onu_fim}\n"
            f"Total configurado: {total} ONU(s)\n\n"
            f"Veja o resultado na aba 'Script Gerado'."
        )
        self.nb.select(self.tab_script)

    def _copiar(self):
        conteudo = self.text_script.get("1.0", tk.END).strip()
        if not conteudo:
            messagebox.showwarning("Aviso", "Nenhum script gerado ainda.")
            return
        self.clipboard_clear()
        self.clipboard_append(conteudo)
        messagebox.showinfo("Copiado", "Script copiado para a area de transferencia!")

    def _salvar_arquivo(self):
        conteudo = self.text_script.get("1.0", tk.END).strip()
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
            filetypes=[("Arquivo de Texto", "*.txt"), ("Todos os Arquivos", "*.*")]
        )
        if not caminho:
            return
        try:
            with open(caminho, "w", encoding="utf-8") as f:
                f.write(conteudo)
            self._ultima_pasta = os.path.dirname(caminho)
            messagebox.showinfo("Salvo", f"Arquivo salvo:\n{caminho}")
        except Exception as e:
            messagebox.showerror("Erro", f"Erro ao salvar o arquivo:\n{e}")

    def _abrir_pasta_atual(self):
        abrir_pasta(self._ultima_pasta)

    def _dados_perfil(self):
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

    def _aplicar_perfil(self, dados):
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

    def _salvar_perfil(self):
        caminho = filedialog.asksaveasfilename(
            defaultextension=".json",
            initialfile=CONFIG_FILE,
            filetypes=[("Perfil JSON", "*.json"), ("Todos os Arquivos", "*.*")]
        )
        if not caminho:
            return
        try:
            with open(caminho, "w", encoding="utf-8") as f:
                json.dump(self._dados_perfil(), f, indent=2, ensure_ascii=False)
            messagebox.showinfo("Salvo", f"Perfil salvo em:\n{caminho}")
        except Exception as e:
            messagebox.showerror("Erro", f"Erro ao salvar perfil:\n{e}")

    def _carregar_perfil(self):
        caminho = filedialog.askopenfilename(
            filetypes=[("Perfil JSON", "*.json"), ("Todos os Arquivos", "*.*")]
        )
        if not caminho:
            return
        try:
            with open(caminho, encoding="utf-8") as f:
                self._aplicar_perfil(json.load(f))
            messagebox.showinfo("Carregado", "Perfil carregado com sucesso!")
        except Exception as e:
            messagebox.showerror("Erro", f"Erro ao carregar perfil:\n{e}")

    def _carregar_perfil_auto(self):
        if os.path.exists(CONFIG_FILE):
            try:
                with open(CONFIG_FILE, encoding="utf-8") as f:
                    self._aplicar_perfil(json.load(f))
            except Exception:
                pass


if __name__ == "__main__":
    app = App()
    app.mainloop()
