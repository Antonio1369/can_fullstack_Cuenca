'use client'; // Directiva para cliente

import * as React from 'react';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import { CircularProgress } from '@mui/material';

export default function Page(): React.JSX.Element {
  const [query, setQuery] = React.useState<string>(''); // Consulta del usuario
  const [history, setHistory] = React.useState<{ user: string, bot: string | null }[]>([]); // Historial de mensajes
  const [loading, setLoading] = React.useState<boolean>(false); // Para mostrar el indicador de carga

  // Función para enviar la consulta al backend
  const handleSendQuery = async () => {
    if (!query.trim()) return;

    // Agregar el mensaje del usuario al historial, sin la respuesta del bot
    setHistory((prevHistory) => [
      ...prevHistory,
      { user: query, bot: null }, // solo el mensaje del usuario (sin respuesta aún)
    ]);
    setQuery(''); // Limpiar la consulta del input

    setLoading(true); // Mostrar el cargando mientras esperamos la respuesta de la API

    try {
      const response = await fetch('http://localhost:5000/api/rag/query', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query })
      });
      const data = await response.json();
      const botResponse = data.answer || 'No se obtuvo respuesta.';

      // Agregar la respuesta del bot al historial después de obtenerla
      setHistory((prevHistory) => [
        ...prevHistory.slice(0, prevHistory.length - 1), // Eliminar el mensaje de usuario anterior
        { user: query, bot: botResponse }, // Agregar la respuesta del bot
      ]);
    } catch (error) {
      console.error('Error al obtener respuesta:', error);
      setHistory((prevHistory) => [
        ...prevHistory.slice(0, prevHistory.length - 1),
        { user: query, bot: 'Error en la consulta.' }, // Agregar respuesta de error
      ]);
    } finally {
      setLoading(false); // Ocultar el indicador de carga
    }
  };

  // Función para manejar el evento Enter
  const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'Enter') {
      event.preventDefault(); // Prevenir la acción predeterminada de Enter (por ejemplo, enviar el formulario)
      handleSendQuery(); // Enviar el mensaje
    }
  };

  return (
    <Box sx={{ maxWidth: 900, margin: '0 auto', padding: 3 }}>
      <Stack spacing={3}>
        {/* Cabecera del chat */}
        <Typography variant="h4" align="center" sx={{ fontWeight: 'bold' }}>
          Chatbot - Consultas
        </Typography>

        {/* Contenedor de mensajes */}
        <Box
          sx={{
            height: '400px',
            overflowY: 'auto',
            border: '1px solid #ddd',
            borderRadius: 2,
            padding: 2,
            backgroundColor: '#f9f9f9',
          }}
        >
          {history.map((message, index) => (
            <Box key={index} sx={{ marginBottom: 2 }}>
              <Typography variant="body2" sx={{ fontWeight: 'bold' }}>Usuario:</Typography>
              <Typography variant="body1" sx={{ marginLeft: 2, backgroundColor: '#e1f5fe', borderRadius: 1, padding: 1 }}>
                {message.user}
              </Typography>
              <Typography variant="body2" sx={{ fontWeight: 'bold', marginTop: 1 }}>Bot:</Typography>
              {/* Mostramos el mensaje del bot solo si está disponible */}
              <Typography variant="body1" sx={{ marginLeft: 2, backgroundColor: '#f1f8e9', borderRadius: 1, padding: 1 }}>
                {message.bot ? message.bot : (loading ? <CircularProgress size={20} /> : '')}
              </Typography>
            </Box>
          ))}
        </Box>

        {/* Input y Botón para enviar consulta */}
        <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
          <TextField
            fullWidth
            variant="outlined"
            label="Escribe tu consulta..."
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown} // Detectar presion de Enter
            disabled={loading}
          />
          <Button
            variant="contained"
            color="primary"
            onClick={handleSendQuery}
            disabled={loading}
          >
            Enviar
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
