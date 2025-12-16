import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { ChangeDetectorRef } from '@angular/core';
import { AuthService } from './core/services/auth.service';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatButtonModule,
    CommonModule,
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  year = new Date().getFullYear();
  isLoggedIn = false;

  constructor(private auth: AuthService, private cdr: ChangeDetectorRef) {
    this.isLoggedIn = this.auth.isLoggedIn();

    this.auth.token$.subscribe(() => {
      this.isLoggedIn = this.auth.isLoggedIn();
      this.cdr.detectChanges();
    });
  }
  logout(): void {
    this.auth.logout();
    this.cdr.detectChanges();
  }
}
