# SeedBackend_V1
# SOBRE O DB:
**Só copiar o script dentro do PgAdmin (DBeaver ou outro DBMS de sua escolha) e ficar feliz.**

## - Requisitos

    PostgreSQL 14+ (recomendado)

    Schema padrão: public

## - Convenções

    snake_case, PKs *_id

    FKs nomeadas (fk_<tabela>_<coluna>)

    Regras de negócio via UNIQUE, CHECK e triggers

    Cascata apenas onde faz sentido (ex.: apagar combo apaga seus itens_combo)

## - Tabelas (resumo)

    perfil: catálogo de perfis (tipo_perfil UNIQUE, is_ativo).

    permissoes: catálogo de permissões atômicas (codigo UNIQUE).

    permissao_perfil: N:N entre perfil e permissões (UNIQUE (perfil_id, perm_id)).

    tipo_setor: vocabulário controlado (SEED, DEP, DR, IE, OUTRO).

    setor: unidade organizacional; hierarquia via setor_pai_id (self-FK). IE é apenas um tipo de setor.

    setor_historico: snapshots antes de UPDATE em setor (guarda tipo_setor como código e endereco hoje fixo como NULL).

    usuario: usuários vinculados a perfil e setor.

    usuario_historico: snapshots antes de UPDATE em usuario (grava tipo_perfil textual).

    competencia: janela contábil (ano/mês). aberta_por obrigatório; fechada_por opcional.

    item: catálogo de itens (sem preço; valores consolidados em combo).

    combo: pacotes por setor, com valor_combo e validade.

    itens_combo: itens dentro do combo (UNIQUE (combo_id, item_id)).

    solicitacao_compra: pedidos avulsos por competência (fora do combo).

    gastos: lançamentos; podem referenciar combo ou solicitacao_compra (ambos opcionais).

    folha_pagamento: total por setor/competência; pode referenciar itens_combo.

    alunos_por_setor_competencia: contagem de alunos; só permitido para setores do tipo IE (validado por trigger).

## - Principais FKs e regras

    usuario.setor_id → setor.setor_id

    usuario.perfil_id → perfil.perfil_id

    setor.tipo_setor_id → tipo_setor.tipo_setor_id

    setor.setor_pai_id → setor.setor_id (ON DELETE SET NULL)

    setor.representante_id → usuario.user_id (ON DELETE SET NULL, criada via ALTER TABLE após usuario)

    permissao_perfil (perfil_id, perm_id) UNIQUE para impedir duplicatas

    competencia.ck_abre_fecha: se ambas as datas existem, fechamento >= abertura

## - Triggers

    f_trg_setor_hist + trg_setor_hist_bu
      Snapshot antes de UPDATE em setor. Recupera tipo_setor.codigo, preenche endereco=NULL.

    f_trg_usuario_hist + trg_usuario_hist_bu
      Snapshot antes de UPDATE em usuario. Grava tipo_perfil textual.

    f_ck_alunos_somente_ie + trg_alunos_ie_bi
      Valida que apenas setores com tipo_setor.codigo = 'IE' podem registrar em alunos_por_setor_competencia.

# SOBRE O BACKEND:
