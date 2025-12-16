import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  loading = false;
  error = '';
  hidePassword = true;

  form; // объявили без инициализации

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.nonNullable.group({
      email: [''],
      password: [''],
    });
  };


  

  submit(): void {
    this.error = '';
    this.loading = true;
    this.cdr.detectChanges();

    const dto = this.form.getRawValue(); // { email, password }

    this.auth.login(dto)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: () => {
          this.cdr.detectChanges();
          this.router.navigateByUrl('/products');
        },
        error: (e) => {
          this.error =
            e?.status === 401 ? 'Неверный email или пароль' :
            (e?.error ?? e?.message ?? 'Ошибка входа').toString();

          this.cdr.detectChanges();
        },
      });
  }

  togglePassword(): void {
    this.hidePassword = !this.hidePassword;
    this.cdr.detectChanges();
  }
}
