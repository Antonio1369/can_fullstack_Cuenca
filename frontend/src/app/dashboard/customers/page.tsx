'use client'; 
import * as React from 'react';
import Stack from '@mui/material/Stack';
import Button from '@mui/material/Button';
import { Plus as PlusIcon } from '@phosphor-icons/react/dist/ssr/Plus';
import { FilesFilters } from '@/components/dashboard/customer/customers-filters';
import { FilesTable } from '@/components/dashboard/customer/customers-table';
import { Typography } from '@mui/material';

export default function Page(): React.JSX.Element {
  const [files, setFiles] = React.useState<any[]>([]);  // Estado para almacenar los archivos
  const [loading, setLoading] = React.useState<boolean>(true);  // Estado para manejar la carga
  const page = 0;
  const rowsPerPage = 5;

  React.useEffect(() => {
    async function fetchFiles() {
      try {
        const response = await fetch('http://localhost:5000/api/userfiles?userId=3');
        const data = await response.json();
        setFiles(data);  
      } catch (error) {
        console.error('Error fetching files:', error);
      } finally {
        setLoading(false);
      }
    }

    fetchFiles();
  }, []);

  const paginatedFiles = files.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage);

  return (
    <Stack spacing={3}>
      <Stack direction="row" spacing={3}>
        <Stack spacing={1} sx={{ flex: '1 1 auto' }}>
          <Button startIcon={<PlusIcon fontSize="var(--icon-fontSize-md)" />} variant="contained">
            Add File
          </Button>
        </Stack>
      </Stack>
      <FilesFilters />
      {loading ? (
        <Typography>Loading...</Typography>  // Mostrar mensaje mientras se cargan los archivos
      ) : (
        <FilesTable
          count={files.length}
          page={page}
          rows={paginatedFiles}
          rowsPerPage={rowsPerPage}
        />
      )}
    </Stack>
  );
}
