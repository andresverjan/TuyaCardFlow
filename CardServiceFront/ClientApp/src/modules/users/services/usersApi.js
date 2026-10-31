import { apiRequest } from '../../../shared/api/client';

export const usersApi = { 
    create: values => apiRequest('/users', { method: 'POST', body: values }) 
};
