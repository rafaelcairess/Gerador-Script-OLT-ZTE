Gerador de Scripts de Provisionamento — OLT ZTE Titan
📌 Sobre o Projeto

Ferramenta desktop com interface gráfica (GUI) desenvolvida em Python para automatizar a geração de scripts de provisionamento em massa para OLTs ZTE Titan.

O projeto foi criado para atender demandas reais do NOC, especialmente em cenários de:

migração de clientes entre OLTs

ativação de novas portas PON

janelas de manutenção com tempo crítico

O objetivo é reduzir erros manuais, aumentar a velocidade operacional e minimizar downtime.

 Funcionalidades

Provisionamento em massa: geração automática de configuração para até 128 ONUs por PON

Interface gráfica (GUI): desenvolvida com Tkinter para facilitar o uso em ambientes operacionais

Input dinâmico: definição de PON e VLAN conforme o cenário da rede

Validação de dados: verificação de faixa válida para PON e VLAN, evitando erros de sintaxe

Portabilidade: aplicação compilada em .exe, sem necessidade de ambiente Python instalado

Como usar

Execute o arquivo Gerador OLT.exe

Insira o número da PON de destino

Insira a VLAN de serviço

Clique em GERAR SCRIPT

O sistema irá gerar um arquivo .txt com os comandos CLI

Copie o conteúdo e aplique no terminal da OLT ZTE Titan

🛠️ Tecnologias Utilizadas

Linguagem: Python 3

Interface Gráfica: Tkinter

Compilação: PyInstaller

Versionamento: Git & GitHub

 Evolução do Projeto

Inicialmente, o processo de migração era feito através da edição manual de arquivos em lote (.bat), o que se mostrava lento e altamente propenso a erros.

Vantagens da versão em Python

Eficiência: geração instantânea de scripts complexos

Confiabilidade: redução de erros humanos durante a configuração

Escalabilidade: código preparado para novas regras de negócio e futuras melhorias
