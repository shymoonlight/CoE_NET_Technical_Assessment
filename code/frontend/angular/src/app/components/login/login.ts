import { Component, ChangeDetectorRef } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { LoginService } from '../../services/login.service';
import { AuthResult } from '../../models/AuthResultModel';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  isSubmitting = false;
  errorMessage = '';

  form = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  constructor(private fb: FormBuilder, private loginService: LoginService, private router: Router, private cdr: ChangeDetectorRef) {}

  submit(): void {
    if (this.form.invalid || this.isSubmitting) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const payload = this.form.getRawValue() as { username: string; password: string };

    this.loginService.login(payload).subscribe({
     next: (res: AuthResult) => {
        this.isSubmitting = false;
       
        if (res.token) {
          localStorage.setItem('auth_token', res.token);
          if (res.user) {
            localStorage.setItem('auth_user', JSON.stringify(res.user));
          }
          this.router.navigate(['/users']);
        } else {
          console.log('No token in response', res);
          this.errorMessage = 'Invalid credentials. Please try again.';
        }
      },
      error: () => {
        console.log('Login request failed');
        this.isSubmitting = false;
        this.errorMessage = 'Invalid credentials. Please try again.';
        this.cdr.markForCheck();
      },
    });
  }
}
