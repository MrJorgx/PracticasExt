-- Crear tabla clientes
CREATE TABLE clientes (
    dni VARCHAR(9) PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    tipo_cliente VARCHAR(20) NOT NULL CHECK (tipo_cliente IN ('REGISTRADO', 'SOCIO')),
    cuota_maxima DECIMAL(10,2) NULL,
    fecha_alta TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Crear tabla recibos
CREATE TABLE recibos (
    numero_recibo VARCHAR(50) PRIMARY KEY,
    dni_cliente VARCHAR(9) NOT NULL,
    importe DECIMAL(10,2) NOT NULL CHECK (importe > 0),
    fecha_emision TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (dni_cliente) REFERENCES clientes(dni) ON DELETE CASCADE
);

-- Crear índices para mejorar rendimiento
CREATE INDEX idx_clientes_fecha_alta ON clientes(fecha_alta);
CREATE INDEX idx_recibos_fecha_emision ON recibos(fecha_emision);
CREATE INDEX idx_recibos_dni_cliente ON recibos(dni_cliente);