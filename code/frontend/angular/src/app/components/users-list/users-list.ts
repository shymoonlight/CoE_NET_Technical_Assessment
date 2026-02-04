import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { UsersService } from '../../services/users.service';
import { UserDto } from '../../models/UserDtoModel';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './users-list.html',
  styleUrl: './users-list.css',
})
export class UsersList {
  users: UserDto[] = [];
  isSubmitting = false;
  errorMessage = '';
  currentUserId: string | null = null;
  currentUserRole: string | null = null;
  isEditing = false;
  isCreating = false;

  form = this.fb.group({
    id: [''],
    name: ['', [Validators.required]],
    userName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    role: ['', [Validators.required]],
  });

  constructor(
    private fb: FormBuilder,
    private usersService: UsersService,
    private cdr: ChangeDetectorRef
  ) {
    this.loadAuthUser();
    this.loadUsers();
  }

  get isAdmin(): boolean {
    return this.currentUserRole === 'Admin';
  }

  get isUserRole(): boolean {
    return this.currentUserRole === 'User';
  }

  get showForm(): boolean {
    return this.isEditing || this.isCreating;
  }

  private loadAuthUser(): void {
    const raw = localStorage.getItem('auth_user');
    if (!raw) return;

    try {
      const parsed = JSON.parse(raw) as { id?: number | string; role?: string };
      this.currentUserId = parsed.id !== undefined ? String(parsed.id) : null;
      this.currentUserRole = parsed.role ?? null;

      if (this.isUserRole) {
        this.form.get('role')?.disable({ emitEvent: false });
      }
    } catch {
      this.currentUserId = null;
      this.currentUserRole = null;
    }
  }

  loadUsers(): void {
    this.usersService.getUsers().subscribe({
      next: (data) => { 
        this.users = data;
        this.cdr.markForCheck();
      },
      error: () => (this.errorMessage = 'Failed to load users.'),
    });
  }

  edit(user: UserDto): void {
    if (!this.canEdit(user)) {
      this.errorMessage = 'You are not allowed to edit this user.';
      return;
    }

    this.isEditing = true;
    this.isCreating = false;

    this.form.patchValue({
      id: user.id,
      name: user.name,
      userName: user.userName,
      email: user.email,
      password: '',
      role: user.role,
    });

    if (this.isUserRole) {
      this.form.get('role')?.disable({ emitEvent: false });
    } else {
      this.form.get('role')?.enable({ emitEvent: false });
    }
  }

  resetForm(keepMode = false): void {
    this.form.reset({
      id: '',
      name: '',
      userName: '',
      email: '',
      password: '',
      role: '',
    });

    if (!keepMode) {
      this.isEditing = false;
      this.isCreating = false;
    }

    if (this.isUserRole) {
      this.isEditing = false;
    }

    if (this.isUserRole) {
      this.form.get('role')?.disable({ emitEvent: false });
    } else {
      this.form.get('role')?.enable({ emitEvent: false });
    }
  }

  startCreate(): void {
    if (!this.canCreate()) {
      this.errorMessage = 'You are not allowed to create users.';
      return;
    }

    this.errorMessage = '';
    this.isCreating = true;
    this.isEditing = false;
    this.resetForm(true);
  }

  save(): void {
    if (this.form.invalid || this.isSubmitting) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    const id = this.form.value.id || '';
    if (!id && !this.canCreate()) {
      this.isSubmitting = false;
      this.errorMessage = 'You are not allowed to create users.';
      return;
    }

    if (id && !this.canEdit({ id } as UserDto)) {
      this.isSubmitting = false;
      this.errorMessage = 'You are not allowed to edit this user.';
      return;
    }

    const payload = {
      name: this.form.value.name!,
      email: this.form.value.email!,
      password: this.form.value.password!,
      userName: this.form.value.userName!,
      role: this.isAdmin ? this.form.value.role! : this.currentUserRole ?? 'User',
    };

    const request$ = id
      ? this.usersService.updateUser(id, payload)
      : this.usersService.createUser(payload);

    request$.subscribe({
      next: () => {
        this.isSubmitting = false;
        this.resetForm();
        this.loadUsers();
      },
      error: () => {
        this.isSubmitting = false;
        this.errorMessage = 'Save failed. Please try again.';
      },
    });
  }

  remove(user: UserDto): void {
    if (!user.id || !this.canDelete(user)) {
      this.errorMessage = 'You are not allowed to delete this user.';
      return;
    }

    const confirmed = window.confirm(`Delete user ${user.name}?`);
    if (!confirmed) return;

    this.usersService.deleteUser(user.id).subscribe({
      next: () => this.loadUsers(),
      error: () => (this.errorMessage = 'Delete failed.'),
    });
  }

  canCreate(): boolean {
    return this.isAdmin;
  }

  canEdit(user: UserDto): boolean {
    if (this.isAdmin) return true;
    if (!this.isUserRole) return false;
    return !!this.currentUserId && user.id === this.currentUserId;
  }

  canDelete(user: UserDto): boolean {
    if (this.isAdmin) return true;
    return false;
  }
}
