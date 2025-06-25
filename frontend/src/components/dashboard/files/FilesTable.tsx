import * as React from 'react';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper } from '@mui/material';

function FilesTable({ rows, count, page, rowsPerPage }: { rows: any[], count: number, page: number, rowsPerPage: number }) {
  return (
    <TableContainer component={Paper}>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Archivo</TableCell>
            <TableCell>Fecha de Carga</TableCell>
            <TableCell>Procesado</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.id}>
              <TableCell>{row.filename}</TableCell>
              <TableCell>{dayjs(row.uploadedAt).format('DD/MM/YYYY HH:mm')}</TableCell>
              <TableCell>{row.processed ? 'Sí' : 'No'}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export { FilesTable };
