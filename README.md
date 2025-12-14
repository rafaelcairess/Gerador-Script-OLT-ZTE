# Gerador de Scripts de Provisionamento OLT ZTE Titan

## Como Usar

### Passo a Passo
1. Execute o arquivo `Gerador OLT.exe`.
2. Insira o número da **PON** de destino.
3. Insira a VLAN de serviço correspondente.
4. Clique em GERAR SCRIPT.
5. O sistema gerará um arquivo `.txt` contendo os comandos CLI para provisionar todas as 128 posições da porta.
6. Copie o conteúdo e aplique no terminal da OLT ZTE.



## Sobre o Projeto

Este projeto consiste em uma ferramenta Desktop com Interface Gráfica (GUI) para gerar scripts de configuração em massa para a OLT ZTE Titan.

A solução foi criada para atender demandas do NOC, especificamente em cenários de **migração de clientes entre OLTs** e ativação de novas portas PON, onde a velocidade e a precisão são cruciais para minimizar o tempo de inatividade (downtime).

### Funcionalidades

- **Provisionamento em Massa:** Gera a configuração completa para 128 ONUs simultaneamente.
- **Interface Visual:** GUI desenvolvida com `tkinter` para agilizar a operação durante janelas de manutenção.
- **Input Dinâmico:** Definição rápida de PON e VLAN para adaptação a diferentes cenários de rede.
- **Segurança Operacional:** Validação de entradas para evitar erros de sintaxe nos comandos enviados à OLT.
- **Portabilidade:** Executável standalone (.exe) para uso em qualquer estação de trabalho.

---

## Tecnologias Utilizadas

- Linguagem: [Python 3](https://www.python.org/)
- Interface Gráfica: Tkinter
- Compilação:** PyInstaller
- Versionamento:** Git & GitHub

---

## Evolução: Scripting para Automação GUI

Anteriormente, as migrações dependiam de edição manual de arquivos em lote (.bat), o que era propenso a erros e lento para grandes volumes.

Vantagens da nova versão Python:
1.  Eficiência: Geração instantânea de scripts complexos.
2.  Confiabilidade: Eliminação de erros de digitação comuns em operações de terminal.
3. Escalabilidade: Código preparado para futuras implementações e novas regras de negócio da rede.

