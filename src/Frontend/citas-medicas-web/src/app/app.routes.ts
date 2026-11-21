import { Routes } from '@angular/router';
import { authGuard, invitadoGuard, rolGuard } from './core/auth.guard';

// Cada pantalla se carga solo cuando se necesita (lazy loading)
export const routes: Routes = [
  { path: 'login', canActivate: [invitadoGuard], loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/layout').then(m => m.Layout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', title: 'Inicio', loadComponent: () => import('./pages/dashboard/dashboard').then(m => m.Dashboard) },
      {
        path: 'pacientes', title: 'Pacientes', canActivate: [rolGuard], data: { roles: ['Administrador', 'Recepcion', 'Medico', 'Enfermeria'] },
        loadComponent: () => import('./pages/pacientes/pacientes').then(m => m.Pacientes)
      },
      {
        path: 'agendar', title: 'Agendar cita', canActivate: [rolGuard], data: { roles: ['Administrador', 'Recepcion', 'Medico'] },
        loadComponent: () => import('./pages/agendar/agendar').then(m => m.Agendar)
      },
      {
        path: 'agenda', title: 'Agenda', canActivate: [rolGuard], data: { roles: ['Administrador', 'Recepcion', 'Medico', 'Enfermeria'] },
        loadComponent: () => import('./pages/agenda/agenda').then(m => m.Agenda)
      },
      {
        path: 'mis-citas', title: 'Mis citas', canActivate: [rolGuard], data: { roles: ['Medico'] },
        loadComponent: () => import('./pages/mis-citas/mis-citas').then(m => m.MisCitas)
      },
      {
        path: 'atencion/:id', title: 'Atención médica', canActivate: [rolGuard], data: { roles: ['Medico', 'Administrador'] },
        loadComponent: () => import('./pages/atencion/atencion').then(m => m.AtencionPage)
      },
      {
        path: 'expediente/:pacienteId', title: 'Expediente clínico', canActivate: [rolGuard], data: { roles: ['Medico', 'Administrador', 'Enfermeria'] },
        loadComponent: () => import('./pages/expediente/expediente').then(m => m.ExpedientePage)
      },
      {
        path: 'compartir', title: 'Compartir mi agenda', canActivate: [rolGuard], data: { roles: ['Medico'] },
        loadComponent: () => import('./pages/compartir/compartir').then(m => m.Compartir)
      },

      // ---------- Panel de Administración (solo Administrador) ----------
      {
        path: 'admin/personal', title: 'Personal', canActivate: [rolGuard], data: { roles: ['Administrador'] },
        loadComponent: () => import('./pages/admin/personal').then(m => m.Personal)
      },
      {
        path: 'admin/horarios', title: 'Horarios', canActivate: [rolGuard], data: { roles: ['Administrador'] },
        loadComponent: () => import('./pages/admin/horarios').then(m => m.AdminHorarios)
      },
      {
        path: 'admin/auditoria', title: 'Auditoría', canActivate: [rolGuard], data: { roles: ['Administrador'] },
        loadComponent: () => import('./pages/admin/auditoria').then(m => m.Auditoria)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
