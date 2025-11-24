import React, { useEffect, useState } from 'react';
import type Chamado from '../models/chamado';

const Cadastrarchamado: React.FC = () => {
  const [chamados, setChamados] = useState<Chamado[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const controller = new AbortController();

    async function loadChamados() {
      setLoading(true);
      setError(null);

      try {
        // Read VITE_API_URL if defined — use a typed cast to avoid `any`
        const meta = (import.meta as unknown) as { env?: { VITE_API_URL?: string } };
        const base = meta.env?.VITE_API_URL ?? '';

        const res = await fetch(`${base}/chamados`, { signal: controller.signal });

        if (!res.ok) {
          throw new Error(`Falha ao buscar chamados: ${res.status} ${res.statusText}`);
        }

        // Assume API returns an array of chamados
        const data: Chamado[] = await res.json();
        setChamados(data);
      } catch (err: unknown) {
        // Handle abort
        if (typeof err === 'object' && err !== null && 'name' in err && (err as { name?: string }).name === 'AbortError') return;

        setError(err instanceof Error ? err.message : String(err));
      } finally {
        setLoading(false);
      }
    }

    loadChamados();
    return () => controller.abort();
  }, []);

  return (
    <div style={{ padding: 16 }}>
      <h2>Lista de Chamados</h2>

      {loading && <p>Carregando chamados...</p>}
      {error && (
        <div style={{ color: 'red', marginBottom: 12 }}>
          <strong>Erro:</strong> {error}
        </div>
      )}

      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th style={{ textAlign: 'left', padding: '8px', borderBottom: '1px solid #ddd' }}>ID</th>
              <th style={{ textAlign: 'left', padding: '8px', borderBottom: '1px solid #ddd' }}>Nome</th>
              <th style={{ textAlign: 'right', padding: '8px', borderBottom: '1px solid #ddd' }}>Quantidade</th>
              <th style={{ textAlign: 'right', padding: '8px', borderBottom: '1px solid #ddd' }}>Preço</th>
              <th style={{ textAlign: 'left', padding: '8px', borderBottom: '1px solid #ddd' }}>Criado Em</th>
            </tr>
          </thead>
          <tbody>
            {chamados.map((c) => (
              <tr key={c.id ?? JSON.stringify(c)}>
                <td style={{ padding: '8px', borderBottom: '1px solid #f0f0f0' }}>{c.id ?? '-'}</td>
                <td style={{ padding: '8px', borderBottom: '1px solid #f0f0f0' }}>{c.nome}</td>
                <td style={{ padding: '8px', borderBottom: '1px solid #f0f0f0', textAlign: 'right' }}>{c.quantidade}</td>
                <td style={{ padding: '8px', borderBottom: '1px solid #f0f0f0', textAlign: 'right' }}>
                  {Number(c.preco).toLocaleString(undefined, { style: 'currency', currency: 'BRL' })}
                </td>
                <td style={{ padding: '8px', borderBottom: '1px solid #f0f0f0' }}>{c.criadoEm ? new Date(c.criadoEm).toLocaleString() : '-'}</td>
              </tr>
            ))}

            {chamados.length === 0 && !loading && (
              <tr>
                <td colSpan={5} style={{ padding: '8px' }}>
                  Nenhum chamado encontrado.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default Cadastrarchamado;