import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { BaseUnitService } from 'src/app/proxy/base-units';
import { BaseUnitDto, GetBaseUnitListDto } from 'src/app/proxy/base-units/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-units',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './units.component.html',
  styleUrl: './units.component.scss',
  providers: [ListService]
})
export class UnitsComponent implements OnInit, OnDestroy{
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
    this.buildForm();
  }

  ngOnInit(): void {
    const unitStreamCreator = (query) => this.unitService.getList({...query, filter: this.filterText});
    this.list.maxResultCount = 10;
    this.list.hookToQuery(unitStreamCreator).subscribe((response) => {
      this.unit = response;
    });
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
    this.buildForm(); 
    this.isDrawerOpen = true;
  }

  editUnit(id: string): void {
    this.unitService.get(id).subscribe((res) => {
      this.selectedUnit = res;
      this.buildForm(); 
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
      code: [this.selectedUnit.code || '', [Validators.required, Validators.maxLength(50)]],
      name: [this.selectedUnit.name || '', [Validators.required, Validators.maxLength(100)]],
    });

    //Logic sinh code khi tạo
    if (!this.selectedUnit.id) {
      this.form.get('name')?.valueChanges
        .pipe(takeUntil(this.destroy$)) // Tự hủy khi component bị hủy
        .subscribe((value) => {
          if (value) {
            const generatedCode = this.generateCode(value);
            // set value cho ô Code, emitEvent: false để tránh vòng lặp vô tận
            this.form.get('code')?.setValue(generatedCode, { emitEvent: false });
          }
        });
    }
  }

  private generateCode(str: string): string {
    if (!str) return '';
    return str
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '') // Bỏ dấu tiếng Việt
      .replace(/đ/g, 'd').replace(/Đ/g, 'D')
      .replace(/[^a-zA-Z0-9 ]/g, '') // Bỏ ký tự đặc biệt
      .trim()
      .replace(/\s+/g, '_') // Thay khoảng trắng bằng _
      .toUpperCase(); // Chuyển thành CHỮ HOA
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
        this.selectedUnit.id? '::UpdateSuccess' : '::CreateSuccess', '::Success'
      );
    });
  }
}
