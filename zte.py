import tkinter as tk
from tkinter import messagebox
import os

def gerar_script():
    pon = entry_pon.get().strip()
    vlan = entry_vlan.get().strip()

    if not pon or not vlan:
        messagebox.showerror("Erro", "Por favor, preencha a PON e a VLAN.")
        return

    nome_arquivo = f"script_pon_{pon}_vlan_{vlan}.txt"

    try:
        with open(nome_arquivo, "w", encoding="utf-8") as f:
            # Cabeçalho
            f.write(f"! ######################################################\n")
            f.write(f"! ### INICIO CONFIGURACAO PON {pon} / VLAN {vlan} ###\n")
            f.write(f"! ######################################################\n\n")

            # Loop de 1 a 128
            for i in range(1, 129):
                # Bloco de configuração da ONU
                bloco = (
                    f"! Configuracao para ONU {i}\n"
                    f"interface gpon_onu-1/3/{pon}:{i}\n"
                    f"name automatico\n"
                    f"description automatico\n"
                    f"tcont 4 profile 1G\n"
                    f"gemport 1 name GEM1 tcont 4\n"
                    f"exit\n"
                    f"interface vport-1/3/{pon}.{i}:1\n"
                    f"service-port 1 user-vlan {vlan} vlan {vlan}\n"
                    f"exit\n"
                    f"pon-onu-mng gpon_onu-1/3/{pon}:{i}\n"
                    f"service 1 gemport 1 vlan {vlan}\n"
                    f"vlan port eth_0/1 mode tag vlan {vlan}\n"
                    f"exit\n\n"
                )
                f.write(bloco)

            f.write(f"! ####################################################\n")
            f.write(f"! ### FIM CONFIGURACAO PON {pon} / VLAN {vlan} ###\n")
            f.write(f"! ####################################################\n")
        messagebox.showinfo("Sucesso", f"Script gerado com sucesso!\nSalvo como: {nome_arquivo}")
        os.startfile(os.getcwd())

    except Exception as e:
        messagebox.showerror("Erro", f"Ocorreu um erro ao gerar o arquivo:\n{e}")

#Interface Gráfica (GUI)

janela = tk.Tk()
janela.title("Gerador OLT ZTE - Barra Connect")
janela.geometry("400x350")
janela.resizable(False, False)
cor_fundo = "#9e9e9e"
cor_botao = "#0056B8"
cor_texto_botao = "white"
janela.configure(bg=cor_fundo)

lbl_titulo = tk.Label(janela, text="Gerador OLT ZTE - Barra Connect", 
                      font=("Arial", 14, "bold"), bg=cor_fundo)
lbl_titulo.pack(pady=20)

lbl_subtitulo = tk.Label(janela, text="OLT ZTE TITAN", 
                         font=("Arial", 10, "italic"), bg=cor_fundo, fg="#666")
lbl_subtitulo.pack(pady=(0, 20))

frame_pon = tk.Frame(janela, bg=cor_fundo)
frame_pon.pack(pady=5)
lbl_pon = tk.Label(frame_pon, text="Número da PON:", font=("Arial", 11), bg=cor_fundo)
lbl_pon.pack(side=tk.LEFT, padx=5)
entry_pon = tk.Entry(frame_pon, width=10, font=("Arial", 11))
entry_pon.pack(side=tk.LEFT)

frame_vlan = tk.Frame(janela, bg=cor_fundo)
frame_vlan.pack(pady=5)
lbl_vlan = tk.Label(frame_vlan, text="Número da VLAN:", font=("Arial", 11), bg=cor_fundo)
lbl_vlan.pack(side=tk.LEFT, padx=5)
entry_vlan = tk.Entry(frame_vlan, width=10, font=("Arial", 11))
entry_vlan.pack(side=tk.LEFT)

btn_gerar = tk.Button(janela, text="GERAR SCRIPT", font=("Arial", 12, "bold"), 
                      bg=cor_botao, fg=cor_texto_botao, height=2, width=20,
                      command=gerar_script)
btn_gerar.pack(pady=30)
lbl_creditos = tk.Label(janela, text="By Rafael Caires & Lucas Alexandre", 
                        font=("Arial", 9), bg=cor_fundo, fg="gray")
lbl_creditos.pack(side=tk.BOTTOM, pady=10)
janela.mainloop()