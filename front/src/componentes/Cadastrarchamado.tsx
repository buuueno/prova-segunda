import { useState } from "react";
import type Chamado from "../models/Chamado";
import axios from "axios";

function CadastrarChamado() {
  const [descricao, setDescricao] = useState("");
  const [status, setStatus] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  function submeterForm(e: any) {
    e.preventDefault();
    enviarChamadoAPI();
  }

  async function enviarChamadoAPI() {
    try {
      const chamado: Chamado = { Descricao: descricao,status: status,
      };
      await axios.post("http://localhost:5000/api/chamado/cadastrar", chamado);
      
      window.location.href = "/";
    } catch (err: any) {
      setError(String(err?.message ?? err));
      console.log("Erro na requisição: " + err);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div id="componente_cadastrar_chamado">
      <h1>Cadastrar Chamado</h1>

      <form onSubmit={submeterForm}>
        <div>
          <label>Descrição:</label>
          <input
            type="text"
            value={descricao}
            onChange={(e: any) => setDescricao(e.target.value)}
            required
          />
        </div>

        <div>
          <label>Status:</label>
          <input
            type="text"
            value={status}
            onChange={(e: any) => setStatus(e.target.value)}
            required
          />
        </div>

        <div>
          <button type="submit" disabled={loading}>
            {loading ? "Enviando..." : "Cadastrar"}
          </button>
        </div>
      </form>
    </div>
  );
}

export default CadastrarChamado;
