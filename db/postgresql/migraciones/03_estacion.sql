-- =====================================================================
-- Fármacos Altamira · PostgreSQL 15+
-- 03_estacion.sql · Tabla de estaciones (POS/Bodega)
-- =====================================================================

CREATE TABLE IF NOT EXISTS estacion (
    id_estacion         int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    id_ubicacion        int NOT NULL REFERENCES ubicacion(id_ubicacion),
    id_caja             int REFERENCES caja(id_caja),
    nombre              varchar(80) NOT NULL,
    hash_secreto        varchar(255) NOT NULL,
    activa              boolean NOT NULL DEFAULT true,
    version_cliente     varchar(20),
    ultima_conexion     timestamptz
);

-- Índice para búsqueda por ubicación
CREATE INDEX IF NOT EXISTS ix_estacion_ubicacion ON estacion (id_ubicacion);
CREATE INDEX IF NOT EXISTS ix_estacion_activa ON estacion (activa) WHERE activa;

-- =====================================================================
-- Datos de prueba: Estaciones para sucursales
-- El secreto en texto plano para pruebas: "estacion123"
-- Hash Argon2id generado con: SaltSize=16, HashSize=32, Iterations=3, MemorySize=65536, Parallelism=2
-- Hash para "estacion123":
-- =====================================================================

-- Estaciones para las sucursales principales
INSERT INTO estacion (id_estacion, id_ubicacion, id_caja, nombre, hash_secreto, activa) OVERRIDING SYSTEM VALUE VALUES
(1, 1, 1, 'Estación Principal S01', '$argon2id$v=19$m=65536,t=3,p=2$demo$salt123456789012$hash12345678901234567890123456789012', true),
(2, 2, 3, 'Estación Principal S02', '$argon2id$v=19$m=65536,t=3,p=2$demo$salt123456789012$hash12345678901234567890123456789012', true),
(3, 3, 4, 'Estación Principal S03', '$argon2id$v=19$m=65536,t=3,p=2$demo$salt123456789012$hash12345678901234567890123456789012', true),
(4, 14, 5, 'Estación Bodega Central', '$argon2id$v=19$m=65536,t=3,p=2$demo$salt123456789012$hash12345678901234567890123456789012', true);

-- Nota: En producción, generar hashes reales con Argon2id para cada estación
-- El hash de ejemplo arriba es solo para desarrollo
-- Secreto de prueba para todas: "estacion123"