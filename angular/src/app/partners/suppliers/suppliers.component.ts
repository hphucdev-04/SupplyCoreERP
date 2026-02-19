import { Component } from '@angular/core';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-suppliers',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './suppliers.html',
  styleUrl: './suppliers.scss'
})
export class SuppliersComponent {

}
