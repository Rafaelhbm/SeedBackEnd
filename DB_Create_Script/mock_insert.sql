-- EU PEDI PARA O GPT GERAR.
    -- ERA MAIS RÁPIDO E PRÁTICO.
        -- PEDI PARA ELE ATIVAR A PGCRYPTO POR SEGURANÇA E PARA TESTAR.

-- 1) habilitar pgcrypto (precisa de superuser/apropriado)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- 2) perfis
INSERT INTO perfil (tipo_perfil) VALUES
                                     ('ADMIN'), ('GESTOR_SEED'), ('RH'), ('DIRETOR_IE'), ('ANALISTA_COMPRAS'), ('USUARIO');

-- 3) catálogo de permissões (exemplos)
INSERT INTO permissoes (codigo, perm_nome, perm_descricao) VALUES
                                                               ('APROVAR_COMBO',       'Aprovar combo',              'Pode aprovar combos'),
                                                               ('SUBMETER_COMBO',      'Submeter combo',             'Pode submeter combos'),
                                                               ('APROVAR_SOLICITACAO', 'Aprovar solicitação',        'Pode aprovar solicitações de compra'),
                                                               ('CADASTRAR_GASTO',     'Cadastrar gasto',            'Pode lançar gastos'),
                                                               ('VER_RELATORIOS',      'Ver relatórios',             'Pode ver relatórios'),
                                                               ('GERENCIAR_USUARIOS',  'Gerenciar usuários',         'Pode gerenciar usuários');

-- 4) mapear permissões essenciais (ADMIN recebe todas)
INSERT INTO permissao_perfil (perfil_id, perm_id)
SELECT p.perfil_id, pm.perm_id
FROM perfil p CROSS JOIN permissoes pm
WHERE p.tipo_perfil = 'ADMIN';

-- GESTOR_SEED
INSERT INTO permissao_perfil (perfil_id, perm_id)
SELECT (SELECT perfil_id FROM perfil WHERE tipo_perfil='GESTOR_SEED'),
       perm_id FROM permissoes WHERE codigo IN ('APROVAR_COMBO','APROVAR_SOLICITACAO','VER_RELATORIOS');

-- DIRETOR_IE
INSERT INTO permissao_perfil (perfil_id, perm_id)
SELECT (SELECT perfil_id FROM perfil WHERE tipo_perfil='DIRETOR_IE'),
       perm_id FROM permissoes WHERE codigo IN ('SUBMETER_COMBO','CADASTRAR_GASTO','VER_RELATORIOS');

-- ANALISTA_COMPRAS
INSERT INTO permissao_perfil (perfil_id, perm_id)
SELECT (SELECT perfil_id FROM perfil WHERE tipo_perfil='ANALISTA_COMPRAS'),
       perm_id FROM permissoes WHERE codigo IN ('APROVAR_SOLICITACAO','VER_RELATORIOS');

-- USUARIO
INSERT INTO permissao_perfil (perfil_id, perm_id)
SELECT (SELECT perfil_id FROM perfil WHERE tipo_perfil='USUARIO'),
       perm_id FROM permissoes WHERE codigo IN ('SUBMETER_COMBO');

-- 5) tipos de setor
INSERT INTO tipo_setor (codigo, nome, descricao) VALUES
                                                     ('SEED','Secretaria','SEED central'),
                                                     ('DEP','Departamento','Departamento'),
                                                     ('DR','Diretoria Regional','Diretoria Regional'),
                                                     ('IE','Instituição de Ensino','Escola'),
                                                     ('OUTRO','Outro','Outro tipo');

-- 6) setores (árvore simples)
INSERT INTO setor (nome, tipo_setor_id, setor_pai_id) VALUES
    ('SEED Central',
     (SELECT tipo_setor_id FROM tipo_setor WHERE codigo='SEED'),
     NULL
    );

INSERT INTO setor (nome, tipo_setor_id, setor_pai_id) VALUES
                                                          ('Departamento de Compras',
                                                           (SELECT tipo_setor_id FROM tipo_setor WHERE codigo='DEP'),
                                                           (SELECT setor_id FROM setor WHERE nome='SEED Central')
                                                          ),
                                                          ('Diretoria Regional Norte',
                                                           (SELECT tipo_setor_id FROM tipo_setor WHERE codigo='DR'),
                                                           (SELECT setor_id FROM setor WHERE nome='SEED Central')
                                                          );

INSERT INTO setor (nome, tipo_setor_id, setor_pai_id) VALUES
                                                          ('IE João Paulo II',
                                                           (SELECT tipo_setor_id FROM tipo_setor WHERE codigo='IE'),
                                                           (SELECT setor_id FROM setor WHERE nome='Diretoria Regional Norte')
                                                          ),
                                                          ('IE Maria das Dores',
                                                           (SELECT tipo_setor_id FROM tipo_setor WHERE codigo='IE'),
                                                           (SELECT setor_id FROM setor WHERE nome='Diretoria Regional Norte')
                                                          );

-- 7) usuários (senha_hash com bcrypt via crypt())
--    Senhas em texto para teste (entre parênteses)
INSERT INTO usuario (nome, perfil_id, dt_cadastro, setor_id, email, senha_hash) VALUES
                                                                                    ('Admin SEED',
                                                                                     (SELECT perfil_id FROM perfil WHERE tipo_perfil='ADMIN'),
                                                                                     CURRENT_DATE,
                                                                                     (SELECT setor_id FROM setor WHERE nome='SEED Central'),
                                                                                     'admin@seed.gov.br',
                                                                                     crypt('Admin@123', gen_salt('bf', 10))  -- (Admin@123)
                                                                                    ),
                                                                                    ('Gestor SEED',
                                                                                     (SELECT perfil_id FROM perfil WHERE tipo_perfil='GESTOR_SEED'),
                                                                                     CURRENT_DATE,
                                                                                     (SELECT setor_id FROM setor WHERE nome='Departamento de Compras'),
                                                                                     'gestor@seed.gov.br',
                                                                                     crypt('Gest0r@123', gen_salt('bf', 10)) -- (Gest0r@123)
                                                                                    ),
                                                                                    ('Diretor IE João Paulo II',
                                                                                     (SELECT perfil_id FROM perfil WHERE tipo_perfil='DIRETOR_IE'),
                                                                                     CURRENT_DATE,
                                                                                     (SELECT setor_id FROM setor WHERE nome='IE João Paulo II'),
                                                                                     'diretor.jp2@seed.gov.br',
                                                                                     crypt('Diret0r@123', gen_salt('bf', 10)) -- (Diret0r@123)
                                                                                    ),
                                                                                    ('Analista Compras',
                                                                                     (SELECT perfil_id FROM perfil WHERE tipo_perfil='ANALISTA_COMPRAS'),
                                                                                     CURRENT_DATE,
                                                                                     (SELECT setor_id FROM setor WHERE nome='Departamento de Compras'),
                                                                                     'analista@seed.gov.br',
                                                                                     crypt('Analist@123', gen_salt('bf', 10)) -- (Analist@123)
                                                                                    ),
                                                                                    ('Usuário IE Maria das Dores',
                                                                                     (SELECT perfil_id FROM perfil WHERE tipo_perfil='USUARIO'),
                                                                                     CURRENT_DATE,
                                                                                     (SELECT setor_id FROM setor WHERE nome='IE Maria das Dores'),
                                                                                     'usuario.iedores@seed.gov.br',
                                                                                     crypt('Usu@rio123', gen_salt('bf', 10)) -- (Usu@rio123)
                                                                                    );

-- 8) define representante da IE (opcional, só pra completar)
UPDATE setor s
SET representante_id = u.user_id
    FROM usuario u
WHERE s.nome = 'IE João Paulo II'
  AND u.email = 'diretor.jp2@seed.gov.br';
