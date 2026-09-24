import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import {
  ContainerComponent,
  HeaderBrandComponent,
  HeaderComponent,
  HeaderNavComponent,
  NavComponent,
  NavItemComponent,
  NavLinkDirective
} from '@coreui/angular';
import { AuthService } from '../../core/services/auth.service';

// Layout base del MVP (guia SDD, seccion 17: pantallas objetivo).
// Se usa una barra de navegacion superior (c-header) en lugar de un c-sidebar
// con estado propio, para mantener el shell simple en esta primera iteracion.
@Component({
  selector: 'app-default-layout',
  standalone: true,
  imports: [
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    ContainerComponent,
    HeaderComponent,
    HeaderBrandComponent,
    HeaderNavComponent,
    NavComponent,
    NavItemComponent,
    NavLinkDirective
  ],
  template: `
    <c-header position="sticky">
      <c-container fluid>
        <c-header-brand routerLink="/dashboard">SIGE</c-header-brand>
        <c-header-nav>
          <c-nav>
            <c-nav-item><a cNavLink routerLink="/dashboard" routerLinkActive="active">Dashboard</a></c-nav-item>
            <c-nav-item><a cNavLink routerLink="/mapa" routerLinkActive="active">Mapa</a></c-nav-item>
            <c-nav-item><a cNavLink routerLink="/emergencias" routerLinkActive="active">Emergencias</a></c-nav-item>
            <c-nav-item><a cNavLink routerLink="/unidades" routerLinkActive="active">Unidades</a></c-nav-item>
            <c-nav-item><a cNavLink routerLink="/despacho" routerLinkActive="active">Despacho</a></c-nav-item>
          </c-nav>
        </c-header-nav>
        <button class="btn btn-sm btn-outline-secondary" (click)="logout()">Salir</button>
      </c-container>
    </c-header>
    <c-container fluid class="py-4">
      <router-outlet></router-outlet>
    </c-container>
  `
})
export class DefaultLayoutComponent {
  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
