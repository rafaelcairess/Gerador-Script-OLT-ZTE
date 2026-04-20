![Python](https://img.shields.io/badge/Python-3.x-blue?style=for-the-badge&logo=python)
![Tkinter](https://img.shields.io/badge/Interface-Tkinter-green?style=for-the-badge)

## Sobre o Projeto

Ferramenta desktop com interface gráfica (GUI) desenvolvida em Python para automatizar a geração de scripts de provisionamento em massa para **OLTs ZTE Titan**.

O projeto foi criado para atender demandas reais de **NOC (Network Operations Center)**, solucionando gargalos em cenários de:

- Migração de clientes entre OLTs
- Ativação de novas portas PON
- Janelas de manutenção com tempo crítico

O objetivo principal é **reduzir erros manuais**, aumentar a velocidade operacional e minimizar o *downtime* durante manutenções.

---

## Evolução: Do .BAT ao Python

Inicialmente, o processo de migração era realizado através da edição manual de arquivos em lote (`.bat`). Esse método se mostrava lento e altamente propenso a erros de digitação.

### Vantagens desta versão em Python:

- **Eficiência:** Geração instantânea de scripts complexos
- **Confiabilidade:** Redução drástica de erros humanos durante a configuração
- **Escalabilidade:** Código modular, preparado para novas regras de negócio e melhorias futuras

---

## Funcionalidades

### Configuração da OLT
- **Slot e Card configuráveis** — não mais fixos; adapta-se a qualquer topologia de chassis
- **Range de PONs** — define PON Inicial e PON Final para gerar script de múltiplas PONs de uma vez
- **Range de ONUs** — define ONU Inicial e ONU Final (não necessariamente sempre 1–128)

### Configuração de Serviço
- **VLAN Principal** configurável
- **Perfil TCONT** selecionável via dropdown: `100M`, `200M`, `500M`, `1G`, `2G`, `1GPON`
- **Prefixo de nome/descrição** personalizável — cada ONU recebe `prefixo_PON_ONU` automaticamente
- **Serviço Adicional (VoIP / IPTV)** — habilita gemport 2 com VLAN separada para um segundo serviço por ONU

### Interface e Usabilidade
- **Preview do script** — aba dedicada com scroll horizontal e vertical para visualizar antes de salvar
- **Copiar para Clipboard** — copia o script completo com um clique
- **Salvar Arquivo** — filedialog com nome sugerido automático (`script_ponX-Y_vlanZ.txt`)
- **Salvar / Carregar Perfil** — persiste todas as configurações em JSON para reutilização
- **Auto-carregamento** — ao abrir o programa, carrega `perfil_config.json` automaticamente se existir
- **Abrir Pasta** — abre a pasta do último arquivo salvo
- **Resumo pós-geração** — exibe o total de ONUs configuradas após gerar o script

### Compatibilidade
- **Multiplataforma** — abre pasta corretamente no Windows, macOS e Linux
- **Portabilidade** — compilável em `.exe` via PyInstaller para uso sem instalação de Python

---

## Tecnologias Utilizadas

- [Python 3](https://www.python.org/) — Linguagem principal
- [Tkinter](https://docs.python.org/3/library/tkinter.html) — Interface gráfica
- [PyInstaller](https://pyinstaller.org/) — Compilação para executável standalone

---

## Como Usar

### Para o Usuário Final (Executável)

1. Execute o arquivo `Gerador OLT.exe`
2. Na aba **Configuração**, preencha:
   - Slot, Card, PON Inicial e PON Final
   - VLAN Principal e Perfil TCONT
   - ONU Inicial e ONU Final
   - Prefixo de nome (opcional)
3. Se necessário, habilite o **Serviço Adicional** e informe a VLAN 2
4. Clique em **GERAR SCRIPT**
5. Visualize o resultado na aba **Script Gerado**
6. Use **Copiar para Clipboard** ou **Salvar Arquivo** para exportar
7. Cole o conteúdo no terminal da OLT ZTE Titan

### Perfis de Configuração

- Clique em **Salvar Perfil** para guardar as configurações atuais em um arquivo `.json`
- Clique em **Carregar Perfil** para restaurar uma configuração salva anteriormente
- O arquivo `perfil_config.json` na mesma pasta do executável é carregado automaticamente na abertura

---

*By Rafael Caires & Lucas Alexandre*
