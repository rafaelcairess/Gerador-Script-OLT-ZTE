import re
from dataclasses import dataclass
from typing import Optional

PERFIS_TCONT = ["100M", "200M", "500M", "1G", "2G", "1GPON"]

MAX_SLOT = 99
MAX_CARD = 99
MAX_PON = 16
MAX_ONU = 128
MAX_VLAN = 4096

_PREFIXO_VALIDO = re.compile(r'^[a-zA-Z0-9_\-]+$')


@dataclass
class ConfiguracaoOLT:
    slot: int
    card: int
    pon_ini: int
    pon_fim: int
    vlan: int
    onu_ini: int
    onu_fim: int
    perfil: str
    prefixo: str
    vlan2: Optional[int] = None


def validar_int(valor: str, nome: str, minimo: int, maximo: int) -> int:
    valor = valor.strip()
    if not valor.isdigit():
        raise ValueError(f"'{nome}' deve ser um numero inteiro.")
    n = int(valor)
    if not (minimo <= n <= maximo):
        raise ValueError(f"'{nome}' deve estar entre {minimo} e {maximo}.")
    return n


def validar_prefixo(prefixo: str) -> str:
    prefixo = prefixo.strip() or "automatico"
    if not _PREFIXO_VALIDO.match(prefixo):
        raise ValueError(
            "Prefixo deve conter apenas letras, numeros, underline (_) ou hifen (-)."
        )
    return prefixo


def gerar_script(cfg: ConfiguracaoOLT) -> str:
    sep = "! " + "#" * 56
    linhas = []
    for pon in range(cfg.pon_ini, cfg.pon_fim + 1):
        linhas += [
            sep,
            f"! ### INICIO CONFIGURACAO PON {pon} / VLAN {cfg.vlan} ###",
            sep,
            "",
        ]
        for i in range(cfg.onu_ini, cfg.onu_fim + 1):
            linhas.append(f"! Configuracao ONU {i}")
            linhas.append(f"interface gpon_onu-{cfg.slot}/{cfg.card}/{pon}:{i}")
            linhas.append(f"name {cfg.prefixo}_{pon}_{i}")
            linhas.append(f"description {cfg.prefixo}_{pon}_{i}")
            linhas.append(f"tcont 4 profile {cfg.perfil}")
            linhas.append("gemport 1 name GEM1 tcont 4")
            if cfg.vlan2:
                linhas.append("gemport 2 name GEM2 tcont 4")
            linhas.append("exit")
            linhas.append(f"interface vport-{cfg.slot}/{cfg.card}/{pon}.{i}:1")
            linhas.append(f"service-port 1 user-vlan {cfg.vlan} vlan {cfg.vlan}")
            if cfg.vlan2:
                linhas.append(f"service-port 2 user-vlan {cfg.vlan2} vlan {cfg.vlan2}")
            linhas.append("exit")
            linhas.append(f"pon-onu-mng gpon_onu-{cfg.slot}/{cfg.card}/{pon}:{i}")
            linhas.append(f"service 1 gemport 1 vlan {cfg.vlan}")
            linhas.append(f"vlan port eth_0/1 mode tag vlan {cfg.vlan}")
            if cfg.vlan2:
                linhas.append(f"service 2 gemport 2 vlan {cfg.vlan2}")
                linhas.append(f"vlan port eth_0/2 mode tag vlan {cfg.vlan2}")
            linhas.append("exit")
            linhas.append("")
        linhas += [
            sep,
            f"! ### FIM CONFIGURACAO PON {pon} / VLAN {cfg.vlan} ###",
            sep,
            "",
        ]
    return "\n".join(linhas)
