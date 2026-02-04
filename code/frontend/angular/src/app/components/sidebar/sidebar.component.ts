import { Component, ChangeDetectorRef } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { UserResponse } from '../../models/UserResponseModel';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent {
  name = '';

  constructor(private router: Router, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    const raw = localStorage.getItem('auth_user');

    if (!raw) return;

    try {
      const parsed = JSON.parse(raw) as UserResponse;
      this.name = parsed.name ?? parsed.name ?? '';
    } catch {
      this.name = '';
    }
  }

  logout(): void {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
