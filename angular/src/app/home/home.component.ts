import { Component, inject } from '@angular/core';
import { AuthService } from '@abp/ng.core';
import { SharedModule } from '../shared/shared.module';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  imports: [SharedModule]
})
export class HomeComponent {

}
