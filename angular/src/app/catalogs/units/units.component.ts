import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { BaseUnitService } from 'src/app/proxy/base-units';
import { BaseUnitDto, GetBaseUnitListDto } from 'src/app/proxy/base-units/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-units',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './units.component.html',
  styleUrl: './units.component.scss',
  providers: [ListService]
})
export class UnitsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  unit = { items: [], totalCount: 0 } as PagedResultDto<BaseUnitDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedUnit = {} as BaseUnitDto;
  filterText = ''
  constructor(
    public readonly list: ListService<GetBaseUnitListDto>,
    private unitService: BaseUnitService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
  }

  ngOnInit(): void {
    const unitStreamCreator = (query) => this.unitService.getList({ ...query, filter: this.filterText });
    this.list.maxResultCount = 10;
    this.list.hookToQuery(unitStreamCreator).subscribe((response) => {
      this.unit = response;
    });
    this.buildForm();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  createUnit(): void {
    this.selectedUnit = {} as BaseUnitDto;
    this.form.reset();
    this.isDrawerOpen = true;
  }

  editUnit(id: string): void {
    this.unitService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedUnit = res;
        this.form.patchValue(res);
        this.isDrawerOpen = true;
      });
  }

  deleteUnit(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.unitService.delete(id).subscribe(() => {
            this.list.get();
            this.toaster.success('::DeleteSuccess', '::Success')
          });
        }
      });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
    });
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save(): void {
    if (this.form.invalid) return;

    const request = this.selectedUnit.id
      ? this.unitService.update(this.selectedUnit.id, this.form.value)
      : this.unitService.create(this.form.value);

    request.subscribe(() => {
      this.closeDrawer();
      this.list.get();
      this.toaster.success(
        this.selectedUnit.id ? '::UpdateSuccess' : '::CreateSuccess', '::Success'
      );
    });
  }
}
