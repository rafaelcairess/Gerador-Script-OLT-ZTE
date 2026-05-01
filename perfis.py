import json
import os
from typing import Optional

CONFIG_FILE = "perfil_config.json"


def salvar_perfil(dados: dict, caminho: str) -> None:
    with open(caminho, "w", encoding="utf-8") as f:
        json.dump(dados, f, indent=2, ensure_ascii=False)


def carregar_perfil(caminho: str) -> dict:
    try:
        with open(caminho, encoding="utf-8") as f:
            return json.load(f)
    except json.JSONDecodeError as e:
        raise ValueError(f"Arquivo de perfil invalido ou corrompido: {e}") from e


def carregar_perfil_auto() -> Optional[dict]:
    if not os.path.exists(CONFIG_FILE):
        return None
    try:
        return carregar_perfil(CONFIG_FILE)
    except (ValueError, OSError):
        return None
