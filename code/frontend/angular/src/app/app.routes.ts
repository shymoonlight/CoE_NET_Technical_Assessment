import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { AboutComponent } from './pages/about/about.component';
import { Login } from './components/login/login';
import { UsersList } from './components/users-list/users-list';
import { redirectAuthGuard } from './guards/redirect-auth.guard';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', component: Login, canActivate: [redirectAuthGuard] },
  { path: 'login', component: Login, canActivate: [redirectAuthGuard] },
  { path: 'users', component: UsersList, canActivate: [authGuard]},
  { path: 'home', component: HomeComponent },
  { path: 'about', component: AboutComponent },
  { path: '**', component: PageNotFoundComponent },
];
