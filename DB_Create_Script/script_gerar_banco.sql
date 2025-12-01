-- PostgreSQL 17
-- OBS: Incorporei as IEs dentro de Setor, para evitar redundância.
-- Ao longo do script, colocarei comentários para explicar-- Caso alguém leia.

-- Primeiro, controle de acesso.
CREATE TABLE perfil (
	perfil_id SERIAL PRIMARY KEY,
	tipo_perfil VARCHAR(20) NOT NULL UNIQUE, -- um único registro de perfil concentra permissões; qualquer ajuste no perfil reflete em todos os usuários que o usam; evita duplicatas como dois “GESTOR_SEED” diferentes.
	is_ativo BOOLEAN NOT NULL DEFAULT TRUE
);
-- Acima, perfis de acesso (Admin, Gestor, RH, outros (caso necessário...))

CREATE TABLE permissoes (
	perm_id SERIAL PRIMARY KEY,
	codigo VARCHAR(60) NOT NULL UNIQUE,
	perm_nome VARCHAR(60) NOT NULL,
	perm_descricao TEXT
);
-- Acima, o catálogo de permissoes que o sistema terá (aprovar_combo, submeter_combo, pedir_solicitacao...)

CREATE TABLE permissao_perfil (
	perm_perf_id SERIAL PRIMARY KEY,
	
	perfil_id INTEGER NOT NULL
		CONSTRAINT fk_permissao_perfil_perfil
		REFERENCES perfil(perfil_id) 
		ON DELETE CASCADE,
		
	perm_id INTEGER NOT NULL
		CONSTRAINT fk_permissao_perfil_perm
		REFERENCES permissoes(perm_id) 
		ON DELETE CASCADE,

	CONSTRAINT uq_permissao_perfil UNIQUE (perfil_id, perm_id)
);
-- Tabela de linkagem N:M entre perfil e permissões

-- Agora, a parte de setor! ( e hierarquia :/ )

CREATE TABLE tipo_setor (
  tipo_setor_id  SERIAL PRIMARY KEY,
  codigo         VARCHAR(20)  NOT NULL UNIQUE, -- ex.: SEED, DEP, DR, IE, OUTRO
  nome           VARCHAR(60)  NOT NULL,        -- rótulo legível
  descricao      TEXT,
  is_ativo       BOOLEAN      NOT NULL DEFAULT TRUE
);

-- agora setor de vdd
CREATE TABLE setor (
	setor_id SERIAL PRIMARY KEY,
	nome VARCHAR(100) NOT NULL,
  	tipo_setor_id INTEGER NOT NULL
 			REFERENCES tipo_setor(tipo_setor_id), -- SEED, DEP, DR, IE...
	representante_id INTEGER, -- Adicionarei após a criação da tabela usuarios, para evitar circularidade.
	setor_pai_id INTEGER
		CONSTRAINT fk_setor_pai
  		REFERENCES setor(setor_id)
 		ON DELETE SET NULL,
	
	CONSTRAINT ck_setor_pai_diff
		CHECK (setor_pai_id IS NULL OR setor_pai_id <> setor_id)
);
CREATE INDEX idx_setor_tipo_id ON setor (tipo_setor_id); -- Botei os indexes para consultas mais 'rápidsa' (mesmo que sejam 'poucas' entradas)
CREATE INDEX idx_setor_pai ON setor(setor_pai_id);

CREATE TABLE setor_historico(
    historico_id BIGSERIAL PRIMARY KEY,
    setor_id INTEGER NOT NULL 
		CONSTRAINT fk_setor_historico_setor
		REFERENCES setor(setor_id) 
		ON DELETE CASCADE,
    nome VARCHAR(255),
    tipo_setor VARCHAR(50),
    endereco VARCHAR(100),
    representante_id INTEGER,
    data_alteracao TIMESTAMPTZ NOT NULL DEFAULT now()
);
COMMENT ON TABLE setor_historico IS 'Guardar trilha de alterações de setores (snapshots, na real)';

-- Agora, usuários (finalmente)
CREATE TABLE usuario (
	user_id SERIAL PRIMARY KEY,
	nome VARCHAR(150) NOT NULL,
	
	perfil_id INTEGER NOT NULL
		CONSTRAINT fk_user_perfil
		REFERENCES perfil(perfil_id),

	dt_cadastro DATE NOT NULL DEFAULT CURRENT_DATE,

	setor_id INTEGER NOT NULL 
		CONSTRAINT fk_user_setor
		REFERENCES setor(setor_id),

	email VARCHAR(100) NOT NULL UNIQUE,
	senha_hash VARCHAR(255) NOT NULL
);
CREATE INDEX idx_usuario_setor  ON usuario(setor_id);
CREATE INDEX idx_usuario_perfil ON usuario(perfil_id);

-- Tabela para usuários (duh)
ALTER TABLE setor -- Adicionar a FK, quando criar a tabela usuário (menconei anteriormente o motivo)
	ADD CONSTRAINT fk_setor_representante
	FOREIGN KEY (representante_id) REFERENCES usuario(user_id) ON DELETE SET NULL;

CREATE TABLE usuario_historico(
    user_hist_id BIGSERIAL PRIMARY KEY,
	
    user_id INTEGER NOT NULL 
		CONSTRAINT fk_user_hist_user
		REFERENCES usuario(user_id) ON DELETE CASCADE,
		
    nome VARCHAR(150),
    tipo_perfil VARCHAR(50), -- snapshot do nome do perfil no momento da alteração
    cadastro DATE,
    setor_id INTEGER,
    data_alteracao TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Agr competencia
CREATE TABLE competencia (
	comp_id SERIAL PRIMARY KEY,
	ano INTEGER NOT NULL,
	mes INTEGER NOT NULL,
	data_abertura DATE,
	data_fechamento DATE,
	
	aberta_por INTEGER NOT NULL 
		CONSTRAINT fk_comp_user_aberta
		REFERENCES usuario(user_id),

	fechada_por INTEGER
		CONSTRAINT fk_comp_user_fechada
		REFERENCES usuario(user_id),

	CONSTRAINT uq_competencia UNIQUE (ano, mes),
	CONSTRAINT ck_mes_valido CHECK (mes BETWEEN 1 AND 12),
	CONSTRAINT ck_abre_fecha CHECK (data_abertura IS NULL OR data_fechamento IS NULL OR data_fechamento >= data_abertura)
);
COMMENT ON TABLE competencia IS 'Janela contábil que o ADM vai decidir';

CREATE TABLE item ( -- Tirei o 'valor' do item, já que valor ficará no combo.
	item_id SERIAL PRIMARY KEY,
	nome VARCHAR(100) NOT NULL,
	descricao TEXT
);
COMMENT ON TABLE item IS 'Catálogo de produtos/serviços para combos e gastos avulsos.';

CREATE TABLE combo (
	combo_id SERIAL PRIMARY KEY,
	descricao TEXT,
	validade_inicio DATE,
	validade_fim DATE,
	valor_combo NUMERIC(14,2) NOT NULL CHECK (valor_combo >= 0),
	
	setor_id INTEGER NOT NULL
		CONSTRAINT fk_combo_setor
		REFERENCES setor(setor_id),

	CONSTRAINT ck_validades CHECK (validade_inicio IS NULL OR validade_fim IS NULL OR validade_fim >= validade_inicio)
);
CREATE INDEX idx_combo_setor    ON combo(setor_id);
-- Combos estão atrelados aos setores, como de se esperar.

CREATE TABLE itens_combo (
	itens_combo_id SERIAL PRIMARY KEY,
	
	combo_id INTEGER NOT NULL
		CONSTRAINT fk_itens_combo_combo
		REFERENCES combo(combo_id) ON DELETE CASCADE,

	item_id INTEGER NOT NULL
		CONSTRAINT fk_itens_combo_item
		REFERENCES item(item_id) ON DELETE CASCADE,

	quantidade INTEGER NOT NULL CHECK (quantidade > 0),

	CONSTRAINT uq_combo_item UNIQUE (combo_id, item_id)
);
COMMENT ON TABLE itens_combo IS 'Itens que compõem o combo atual; Possui chave única para evitar repetições';

CREATE TABLE solicitacao_compra (
    solicitacao_id  SERIAL PRIMARY KEY,
    justificativa TEXT,
	
    comp_id INTEGER NOT NULL 
		CONSTRAINT fk_solic_comp
		REFERENCES competencia(comp_id),
	
    data_solicit TIMESTAMPTZ NOT NULL DEFAULT now(),
    data_analise TIMESTAMPTZ,
    is_aprovado BOOLEAN,
	
    valor_estimado NUMERIC(14,2) 
		CHECK (valor_estimado IS NULL OR valor_estimado >= 0),
		
    requisitor_id INTEGER NOT NULL 
		CONSTRAINT fk_requisitor_usuario
		REFERENCES usuario(user_id),
	
    analista_id INTEGER 
		CONSTRAINT fk_analista_usuario
		REFERENCES usuario(user_id)
);
CREATE INDEX idx_solic_comp     ON solicitacao_compra (comp_id, is_aprovado);

CREATE TABLE gastos (
    gasto_id SERIAL PRIMARY KEY,
    combo_id INTEGER 
		CONSTRAINT fk_gastos_combo
		REFERENCES combo(combo_id),
	
    valor NUMERIC(14,2) NOT NULL 
		CHECK (valor >= 0),
    data_cadastro TIMESTAMPTZ   NOT NULL DEFAULT now(),
	
    comp_id INTEGER NOT NULL 
		CONSTRAINT fk_gastos_comp
		REFERENCES competencia(comp_id),
	
    aprovador_id INTEGER 
		CONSTRAINT fk_gastos_aprovador
		REFERENCES usuario(user_id),
	
    setor_id INTEGER NOT NULL 
		CONSTRAINT fk_gastos_setor
		REFERENCES setor(setor_id),
		
    solicit_compra INTEGER
		CONSTRAINT fk_gastos_solicit_compra
		REFERENCES solicitacao_compra(solicitacao_id)
);
COMMENT ON TABLE gastos IS 'Lançamentos financeiros por competência, setorializados. Podem vincular a combo ou solicitação avulsa.';
CREATE INDEX idx_gastos_comp_setor ON gastos (comp_id, setor_id);
CREATE INDEX idx_gastos_setor ON gastos (setor_id);

-- RH
CREATE TABLE folha_pagamento (
	folha_id SERIAL PRIMARY KEY,
	usuario_registro INTEGER NOT NULL
		CONSTRAINT fk_folha_usuario
		REFERENCES usuario(user_id),
	setor_id INTEGER NOT NULL
		CONSTRAINT fk_folha_setor
		REFERENCES setor(setor_id),
	comp_id INTEGER NOT NULL
		CONSTRAINT fk_folha_comp
		REFERENCES competencia(comp_id),
	valor_total NUMERIC(14,2) NOT NULL
		CHECK (valor_total >= 0),
	data_registro TIMESTAMPTZ NOT NULL DEFAULT now(),
	item_combo_id INTEGER
		CONSTRAINT fk_folha_itemComboID
		REFERENCES itens_combo(itens_combo_id)
);
CREATE INDEX idx_folha_setor_comp ON folha_pagamento (setor_id, comp_id);

-- Alunos por IE, agr
CREATE TABLE alunos_por_setor_competencia (
    setor_id INTEGER NOT NULL 
		CONSTRAINT fk_alunosPorSetor_setor
		REFERENCES setor(setor_id),
	
    comp_id INTEGER NOT NULL
		CONSTRAINT fk_alunosPorSetor_comp
		REFERENCES competencia(comp_id),
	
    quantidade INTEGER NOT NULL 
		CHECK (quantidade >= 0),
	
    usuario_registro INTEGER NOT NULL 
		CONSTRAINT fk_alunosPorSetor_usuario
		REFERENCES usuario(user_id),
	
    CONSTRAINT pk_alunosPorSetorCompetencia PRIMARY KEY (setor_id, comp_id)
);

-- Rian fez essa secão, de solicitações de acesso
CREATE TABLE solicitacao_acesso (
    solicitacao_acesso_id SERIAL PRIMARY KEY,
    nome_completo VARCHAR(150) NOT NULL,
    email VARCHAR(100) NOT NULL,
    justificativa TEXT,
    perfil_desejado_id INTEGER NOT NULL
        REFERENCES perfil(perfil_id),
    status VARCHAR(20) NOT NULL DEFAULT 'PENDENTE', -- PENDENTE, APROVADO, REJEITADO
    data_solicitacao TIMESTAMPTZ NOT NULL DEFAULT now(),
    data_analise TIMESTAMPTZ,
    analisado_por_id INTEGER
        REFERENCES usuario(user_id)
);

COMMENT ON TABLE solicitacao_acesso IS 'Armazena pedidos de novos usuários para acesso ao sistema.';

-- +++++++++++++++++++++++++++++++++
-- Agora, vamo fazer os triggers e compania!
-- +++++++++++++++++++++++++++++++++

-- Snapshot dos setores antes de dar update/alteração
CREATE OR REPLACE FUNCTION f_trg_setor_hist() RETURNS TRIGGER AS $$
DECLARE
	v_tipo_codigo TEXT;
BEGIN
	SELECT ts.codigo INTO v_tipo_codigo
	FROM tipo_setor ts
	WHERE ts.tipo_setor_id = OLD.tipo_setor_id;

	INSERT INTO setor_historico ( setor_id, nome, tipo_setor, endereco, representante_id, data_alteracao )
	VALUES (
    OLD.setor_id,
    OLD.nome,
    v_tipo_codigo,
    NULL, -- setor não possui 'endereco'; manter NULL no histórico
    OLD.representante_id,
    now()
);
	RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_setor_hist_bu ON setor;
CREATE TRIGGER trg_setor_hist_bu
BEFORE UPDATE ON setor
FOR EACH ROW EXECUTE FUNCTION f_trg_setor_hist();

-- Snapshtot dos usuários antes de update/alteração
CREATE OR REPLACE FUNCTION f_trg_usuario_hist() RETURNS TRIGGER AS $$
DECLARE
  v_tipo_perfil TEXT;
BEGIN
  SELECT p.tipo_perfil INTO v_tipo_perfil
  FROM perfil p
  WHERE p.perfil_id = OLD.perfil_id;

  INSERT INTO usuario_historico (
    user_id, nome, tipo_perfil, cadastro, setor_id, data_alteracao
  ) VALUES (
    OLD.user_id,
    OLD.nome,
    v_tipo_perfil,
    OLD.dt_cadastro,
    OLD.setor_id,
    now()
  );

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_usuario_hist_bu ON usuario;
CREATE TRIGGER trg_usuario_hist_bu
BEFORE UPDATE ON usuario
FOR EACH ROW EXECUTE FUNCTION f_trg_usuario_hist();

-- Alunos por competencia (apenas IEs)
CREATE OR REPLACE FUNCTION f_ck_alunos_somente_ie() RETURNS TRIGGER AS $$
DECLARE
  v_codigo TEXT;
BEGIN
  SELECT ts.codigo INTO v_codigo
  FROM tipo_setor ts
  JOIN setor s ON s.tipo_setor_id = ts.tipo_setor_id
  WHERE s.setor_id = NEW.setor_id;

  IF v_codigo <> 'IE' THEN
    RAISE EXCEPTION 'apenas setores do tipo IE podem registrar alunos (setor_id=%)', NEW.setor_id;
  END IF;

  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_alunos_ie_bi ON alunos_por_setor_competencia;
CREATE TRIGGER trg_alunos_ie_bi
BEFORE INSERT OR UPDATE ON alunos_por_setor_competencia
FOR EACH ROW EXECUTE FUNCTION f_ck_alunos_somente_ie();

-- 1. Para 'item' (Adiciona unidade de medida)
ALTER TABLE public.item
ADD COLUMN IF NOT EXISTS unidade_de_medida VARCHAR(255);

-- 2. Para 'competencia' (Adiciona datas novas e CORRIGE O BUG do 'date' vs 'timestamp')
ALTER TABLE public.competencia
ADD COLUMN IF NOT EXISTS data_fim TIMESTAMP,
ADD COLUMN IF NOT EXISTS data_limite TIMESTAMP,
ALTER COLUMN data_abertura TYPE TIMESTAMP,
ALTER COLUMN data_fechamento TYPE TIMESTAMP;

-- 3. Para 'combo' (Remove campos antigos, adiciona o novo)
ALTER TABLE public.combo
DROP COLUMN IF EXISTS valor_combo,
DROP COLUMN IF EXISTS validade_inicio,
DROP COLUMN IF EXISTS validade_fim;

ALTER TABLE public.combo
ADD COLUMN IF NOT EXISTS competencia_id INT;

-- 4. Para 'gastos'
-- Transforma a tabela de gastos para salvar cada item individualmente
ALTER TABLE public.gastos
ADD COLUMN IF NOT EXISTS item_id INTEGER NOT NULL DEFAULT 0,
ADD COLUMN IF NOT EXISTS quantidade INTEGER NOT NULL DEFAULT 0;

-- Cria a segurança para garantir que o item existe
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_gastos_item') THEN
        ALTER TABLE public.gastos
        ADD CONSTRAINT fk_gastos_item FOREIGN KEY (item_id) REFERENCES public.item (item_id);
    END IF;
END $$;