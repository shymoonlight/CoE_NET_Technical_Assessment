import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { UsersList } from './users-list';
import { UsersService } from '../../services/users.service';
import { UserDto } from '../../models/UserDtoModel';

describe('UsersList', () => {
  let component: UsersList;
  let fixture: ComponentFixture<UsersList>;
  let usersService: jasmine.SpyObj<UsersService>;

  beforeEach(async () => {
    usersService = jasmine.createSpyObj<UsersService>('UsersService', [
      'getUsers',
      'createUser',
      'updateUser',
      'deleteUser',
    ]);

    usersService.getUsers.and.returnValue(of([]));

    await TestBed.configureTestingModule({
      imports: [UsersList],
      providers: [{ provide: UsersService, useValue: usersService }],
    })
    .compileComponents();

    fixture = TestBed.createComponent(UsersList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    const users: UserDto[] = [
      {
        id: '1',
        name: 'Admin',
        userName: 'admin',
        email: 'admin@example.com',
        role: 'Admin',
      },
    ];

    usersService.getUsers.and.returnValue(of(users));

    fixture = TestBed.createComponent(UsersList);
    component = fixture.componentInstance;
    fixture.detectChanges();

    expect(component.users.length).toBe(1);
    expect(component.users[0].name).toBe('Admin');
  });

  it('should enter create mode for admin', () => {
    localStorage.setItem('auth_user', JSON.stringify({ id: 1, role: 'Admin' }));

    fixture = TestBed.createComponent(UsersList);
    component = fixture.componentInstance;
    fixture.detectChanges();

    component.startCreate();

    expect(component.isCreating).toBeTrue();
    expect(component.showForm).toBeTrue();
  });

  it('should confirm before deleting', () => {
    localStorage.setItem('auth_user', JSON.stringify({ id: 1, role: 'Admin' }));

    usersService.deleteUser.and.returnValue(of(void 0));
    spyOn(window, 'confirm').and.returnValue(true);

    fixture = TestBed.createComponent(UsersList);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const user: UserDto = {
      id: '2',
      name: 'User Test',
      userName: 'user',
      email: 'user@example.com',
      role: 'User',
    };

    component.remove(user);

    expect(window.confirm).toHaveBeenCalled();
    expect(usersService.deleteUser).toHaveBeenCalledWith('2');
  });
});
