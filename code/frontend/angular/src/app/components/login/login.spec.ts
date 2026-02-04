import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Login } from './login';
import { LoginService } from '../../services/login.service';
import { AuthResult } from '../../models/AuthResultModel';

describe('Login', () => {
  let component: Login;
  let fixture: ComponentFixture<Login>;
  let loginService: jasmine.SpyObj<LoginService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    loginService = jasmine.createSpyObj<LoginService>('LoginService', ['login']);
    router = jasmine.createSpyObj<Router>('Router', ['navigate']);

    await TestBed.configureTestingModule({
      imports: [Login],
      providers: [
        { provide: LoginService, useValue: loginService },
        { provide: Router, useValue: router },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(Login);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should navigate on successful login', () => {
    const response: AuthResult = {
      token: 'token-123',
      expires: new Date(),
      user: {
        id: 1,
        name: 'Admin',
        email: 'admin@example.com',
        userName: 'admin',
        role: 'Admin',
        creationDate: new Date(),
        lastUpdateDate: new Date(),
      },
    };

    loginService.login.and.returnValue(of(response));

    component.form.setValue({ username: 'admin', password: 'password' });
    component.submit();

    expect(localStorage.getItem('auth_token')).toBe('token-123');
    expect(router.navigate).toHaveBeenCalledWith(['/users']);
  });

  it('should show error message on login failure', () => {
    loginService.login.and.returnValue(throwError(() => new Error('fail')));

    component.form.setValue({ username: 'admin', password: 'wrong' });
    component.submit();

    expect(component.errorMessage).toBe('Invalid credentials. Please try again.');
  });
});
