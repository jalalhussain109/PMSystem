import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard } from './core/auth/permission.guard';
import { MainLayoutComponent } from './core/layout/main-layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      {
        path: 'projects',
        canActivate: [permissionGuard],
        data: { permissions: ['Projects.View'] },
        children: [
          { path: 'add', canActivate: [permissionGuard], data: { permissions: ['Projects.Create'] }, loadComponent: () => import('./features/projects/form/project-form.component').then(m => m.ProjectFormComponent) },
          { path: ':id/edit', canActivate: [permissionGuard], data: { permissions: ['Projects.Edit'] }, loadComponent: () => import('./features/projects/form/project-form.component').then(m => m.ProjectFormComponent) },
          { path: ':id/view', canActivate: [permissionGuard], data: { permissions: ['Projects.View'] }, loadComponent: () => import('./features/projects/details/project-details.component').then(m => m.ProjectDetailsComponent) },
          { path: '', pathMatch: 'full', loadComponent: () => import('./features/projects/projects.component').then(m => m.ProjectsComponent) }
        ]
      },
      {
        path: 'tasks',
        canActivate: [permissionGuard],
        data: { permissions: ['Tasks.View'] },
        children: [
          { path: 'add', canActivate: [permissionGuard], data: { permissions: ['Tasks.Create'] }, loadComponent: () => import('./features/tasks/form/task-form.component').then(m => m.TaskFormComponent) },
          { path: ':parentId/subtasks/add', canActivate: [permissionGuard], data: { permissions: ['Tasks.Create'] }, loadComponent: () => import('./features/tasks/form/task-form.component').then(m => m.TaskFormComponent) },
          { path: ':id/edit', canActivate: [permissionGuard], data: { permissions: ['Tasks.Edit'] }, loadComponent: () => import('./features/tasks/form/task-form.component').then(m => m.TaskFormComponent) },
          { path: ':id/view', canActivate: [permissionGuard], data: { permissions: ['Tasks.View'] }, loadComponent: () => import('./features/tasks/details/task-details.component').then(m => m.TaskDetailsComponent) },
          { path: '', pathMatch: 'full', loadComponent: () => import('./features/tasks/tasks.component').then(m => m.TasksComponent) }
        ]
      },
      {
        path: 'users',
        canActivate: [permissionGuard],
        data: { permissions: ['Users.View'] },
        children: [
          {
            path: 'assign-roles',
            canActivate: [permissionGuard],
            data: { roles: ['Admin'] },
            loadComponent: () => import('./features/users/assign-roles/assign-user-role.component').then(m => m.AssignUserRoleComponent)
          },
          { path: '', pathMatch: 'full', loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent) }
        ]
      },
      {
        path: 'roles',
        canActivate: [permissionGuard],
        data: { roles: ['Admin'] },
        loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
