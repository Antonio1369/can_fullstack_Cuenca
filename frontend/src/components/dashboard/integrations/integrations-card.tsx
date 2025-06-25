import * as React from 'react';
import Avatar from '@mui/material/Avatar';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Divider from '@mui/material/Divider';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { Clock as ClockIcon } from '@phosphor-icons/react/dist/ssr/Clock';
import { Download as DownloadIcon } from '@phosphor-icons/react/dist/ssr/Download';
import dayjs from 'dayjs';

export interface Integration {
  id: string;
  title: string;
  description: string;
  logo: string;
  installs: number;
  updatedAt: Date;
}

export interface IntegrationCardProps {
  integration: Integration;
}

export function IntegrationCard({ integration }: IntegrationCardProps): React.JSX.Element {
  return (
    <Card sx={{ backgroundColor: '#fff', borderRadius: 2, boxShadow: 2 }}>
      <CardContent sx={{ paddingBottom: 2 }}>
        <Stack spacing={2}>
          <Box sx={{ display: 'flex', justifyContent: 'flex-start' }}>
            <Avatar src={integration.logo} variant="square" sx={{ width: 40, height: 40 }} />
            <Typography sx={{ marginLeft: 2, fontWeight: 'bold' }} variant="body1">
              {integration.title}
            </Typography>
          </Box>
          <Typography variant="body2">{integration.description}</Typography>
        </Stack>
      </CardContent>
      <Divider />
      <Box sx={{ display: 'flex', justifyContent: 'space-between', padding: 2 }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <ClockIcon fontSize="1rem" />
          <Typography color="text.secondary" variant="body2">
            Updated {dayjs(integration.updatedAt).format('MMM D, YYYY')}
          </Typography>
        </Stack>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
          <DownloadIcon fontSize="1rem" />
          <Typography color="text.secondary" variant="body2">
            {integration.installs} installs
          </Typography>
        </Stack>
      </Box>
    </Card>
  );
}
