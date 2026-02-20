![Python](https://img.shields.io/badge/Python-3.x-blue?style=for-the-badge&logo=python)
![Tkinter](https://img.shields.io/badge/Interface-Tkinter-green?style=for-the-badge)


## 📌 Sobre o Projeto

Ferramenta desktop com interface gráfica (GUI) desenvolvida em Python para automatizar a geração de scripts de provisionamento em massa para **OLTs ZTE Titan**.

O projeto foi criado para atender demandas reais de **NOC (Network Operations Center)**, solucionando gargalos em cenários de:
* 🔄 Migração de clientes entre OLTs.
* 🔌 Ativação de novas portas PON.
* ⏱️ Janelas de manutenção com tempo crítico.

O objetivo principal é **reduzir erros manuais**, aumentar a velocidade operacional e minimizar o *downtime* durante manutenções.

---

## 🚀 Evolução: Do .BAT ao Python

Inicialmente, o processo de migração era realizado através da edição manual de arquivos em lote (`.bat`). Esse método se mostrava lento e altamente propenso a erros de digitação.

### Vantagens desta versão em Python:
* **Eficiência:** Geração instantânea de scripts complexos.
* **Confiabilidade:** Redução drástica de erros humanos (sintaxe) durante a configuração.
* **Escalabilidade:** Código modular, preparado para novas regras de negócio e melhorias futuras.

---

## ✨ Funcionalidades

* ✅ **Provisionamento em massa:** Geração automática de configuração para até **128 ONUs** por PON.
* ✅ **Interface Gráfica (GUI):** Desenvolvida com Tkinter, intuitiva para uso em ambientes operacionais.
* ✅ **Input Dinâmico:** Definição personalizada de PON e VLAN conforme o cenário da rede.
* ✅ **Validação de Dados:** Verificação automática de faixas válidas para PON e VLAN, prevenindo erros de sintaxe antes da aplicação.
* ✅ **Portabilidade:** Aplicação compilada em `.exe` (via PyInstaller), dispensando a instalação de Python na máquina do usuário final.

---

## 🛠️ Tecnologias Utilizadas

* [Python 3](https://www.python.org/) - Linguagem principal.
* [Tkinter](https://docs.python.org/3/library/tkinter.html) - Biblioteca para construção da Interface Gráfica.
* [PyInstaller](https://pyinstaller.org/) - Utilizado para compilar o script em um executável standalone.

---

## 📦 Como Usar

### Para o Usuário Final (Executável)

1. Execute o arquivo `Gerador OLT.exe`.
2. Insira o número da **PON de destino**.
3. Insira a **VLAN de serviço**.
4. Clique no botão **GERAR SCRIPT**.
5. O sistema irá gerar um arquivo `.txt` contendo todos os comandos CLI necessários.
6. Copie o conteúdo do arquivo e aplique no terminal da OLT ZTE Titan.
