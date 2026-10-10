import { Component } from '@angular/core';

@Component({
  selector: 'app-users',
  standalone: true,
  template: `
    <div class="page-heading"><div><span class="eyebrow">ADMINISTRATION</span><h2>Users & access</h2>
    <p class="muted">Manage access through your ASP.NET Core Identity API.</p>
  </div>
   `
})
export class UsersComponent {}
