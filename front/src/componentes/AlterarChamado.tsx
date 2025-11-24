import { useEffect, useState } from "react";
import type Chamado from "../models/Chamado";
import axios from "axios";
import { useNavigate, useParams } from "react-router-dom";

function AlterarChamado() {
	const [descricao, setDescricao] = useState("");
	const [status, setStatus] = useState("");
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const navigate = useNavigate();
	const { id } = useParams();

	useEffect(() => {
		buscarChamado();
	}, [id]);

	async function buscarChamado() {
		try {
			const resposta = await axios.get<Chamado>(`http://localhost:5000/api/chamado/${id}`);
			setDescricao(resposta.data.Descricao);
			setStatus(resposta.data.status);
		} catch (err) {
			setError("Erro ao buscar chamado");
		}
	}

	function submeterForm(e: any) {
		e.preventDefault();
		alterarChamadoAPI();
	}

	async function alterarChamadoAPI() {
		try {
			setLoading(true);
			setError(null);
			const chamado: Chamado = {chamadoId: id, Descricao: descricao, status: status,
			};
			await axios.put(`http://localhost:5000/api/chamado/alterar/${id}`, chamado);
			navigate("/");
		} catch (err: any) {
			setError(String(err?.message ?? err));
		} finally {
			setLoading(false);
		}
	}

	return (
		<div>
			<h1>Alterar Chamado</h1>
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
						{loading ? "Alterando..." : "Alterar"}
					</button>
				</div>
				{error && <p>Erro: {error}</p>}
			</form>
		</div>
	);
}

export default AlterarChamado;