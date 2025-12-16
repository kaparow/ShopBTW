import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <h2>Login</h2>

    <div style="display:grid; gap:8px; max-width:320px;">
      <input placeholder="Email" [(ngModel)]="email" />
      <input placeholder="Password" type="password" [(ngModel)]="password" />
      <button (click)="login()">Войти</button>

      <p style="color:#c00" *ngIf="error">{{error}}</p>
    </div>
  `
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  login() {
    this.error = '';
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: (e) => this.error = (e?.error ?? 'Login failed').toString()
    });
  }
} 
