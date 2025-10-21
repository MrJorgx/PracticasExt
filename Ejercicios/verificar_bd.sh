#!/bin/bash
echo "=== Verificando PostgreSQL ==="
sudo systemctl status postgresql --no-pager

echo -e "\n=== Conectando a la base de datos ==="
psql -h localhost -U ejercicios_user -d ejercicios_db -c "\dt"

echo -e "\n=== Contando registros ==="
psql -h localhost -U ejercicios_user -d ejercicios_db -c "SELECT 'Clientes:', COUNT(*) FROM clientes UNION ALL SELECT 'Recibos:', COUNT(*) FROM recibos;"

echo -e "\n=== Últimos clientes creados ==="
psql -h localhost -U ejercicios_user -d ejercicios_db -c "SELECT dni, nombre, apellidos, tipo_cliente, fecha_alta FROM clientes ORDER BY fecha_alta DESC LIMIT 5;"
