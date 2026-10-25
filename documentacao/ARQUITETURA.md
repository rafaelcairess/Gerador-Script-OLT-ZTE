# Arquitetura

## Objetivo

Manter as regras de provisionamento testáveis e independentes da tecnologia de
interface ou do formato usado para persistir arquivos.

O projeto utiliza uma arquitetura em camadas proporcional ao seu tamanho. Não
há abstrações para regras que possuem apenas uma implementação quando elas não
representam uma fronteira externa.

## Dependências

```text
GeradorOlt.Apresentacao ───────┐
                          ├──> GeradorOlt.Nucleo
GeradorOlt.Infraestrutura ┘

GeradorOlt.Testes ──> todas as camadas
```

`GeradorOlt.Nucleo` não referencia WPF, JSON ou APIs específicas do Windows.

## Camadas

### Núcleo

Contém:

- `ConfiguracaoOlt`, `ScriptGerado` e `ResumoScript`
- validações e limites operacionais
- `IGeradorScript`
- `IServicoPerfil`
- `IServicoExportacaoScript`

O `GeradorScriptZte` recebe uma configuração válida e produz conteúdo e
métricas em uma única operação determinística.

### Infraestrutura

Implementa as fronteiras externas:

- `ServicoPerfilJson` serializa perfis compatíveis com o formato existente
- `ServicoExportacaoTexto` grava scripts em UTF-8 sem BOM

As duas implementações gravam primeiro em um arquivo temporário na mesma pasta
e só então substituem o destino. Os novos perfis incluem uma versão de esquema;
perfis legados são tratados como versão 1.

Os atributos de serialização ficam em `DocumentoPerfil`, impedindo que detalhes
de JSON vazem para os modelos do núcleo.

### Apresentação

Contém a apresentação WPF:

- `JanelaPrincipal` compõe a tela
- `PainelConfiguracao` apresenta o formulário
- `PainelVisualizacaoScript` apresenta métricas, status e resultado
- `ConfiguracaoViewModel` concentra estado e conversão do formulário
- `PrincipalViewModel` orquestra comandos e serviços

Alterações rápidas do formulário passam por um agendador de 200 ms antes de
atualizar o preview, evitando várias gerações completas durante a digitação. O
comando **Gerar** cancela a espera e atualiza imediatamente.

O code-behind dos painéis contém somente `InitializeComponent`. A janela
principal também intercepta o fechamento para proteger alterações não salvas.

## Fluxo principal

1. Um campo altera `ConfiguracaoViewModel`.
2. O evento `Changed` agenda a atualização no `PrincipalViewModel`.
3. A entrada é convertida em `ConfiguracaoOlt`.
4. `IGeradorScript` valida e gera o script.
5. Propriedades observáveis atualizam preview, métricas e comandos.

Entradas inválidas não geram arquivos e desabilitam copiar/exportar.
Erros numéricos também aparecem diretamente no campo correspondente.

## Composição

`App.xaml.cs` é a composition root. Ela cria as implementações concretas e as
injeta no `PrincipalViewModel`. Um contêiner de DI não foi incluído porque a
quantidade de dependências é pequena e a composição explícita permanece mais
simples.

## Testes

- regras e limites do gerador
- geração do segundo serviço
- proteção de identificadores
- compatibilidade de perfis JSON
- versionamento e substituição atômica de arquivos
- codificação do arquivo exportado
- estado, descarte protegido e comandos do `PrincipalViewModel`

Nos testes da apresentação, integrações como área de transferência e caixas de
diálogo usam implementações controladas. Dessa forma, a lógica pode ser validada
automaticamente sem abrir janelas ou alterar o ambiente do computador.
