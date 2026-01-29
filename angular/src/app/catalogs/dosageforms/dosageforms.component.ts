import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';
import { DosageFormService } from 'src/app/proxy/dosage-forms';
import { DosageFormDto, GetDosageFormListDto } from 'src/app/proxy/dosage-forms/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-dosageforms',
  imports: [SharedModule, DrawerComponent],
  templateUrl: './dosageforms.component.html',
  styleUrl: './dosageforms.component.scss',
  providers: [ListService]
})
export class DosageformsComponent implements OnInit , OnDestroy{
  private destroy$ = new Subject<void>();
  private filterSubject$ = new Subject<string>();
  dosage = {items: [], totalCount: 0 } as PagedResultDto<DosageFormDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedDosage = {} as DosageFormDto;
  filterText = '';

  constructor(
    public readonly list: ListService<GetDosageFormListDto>,
    private dosageService: DosageFormService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService
  ){
     this.buildForm()
  }

  ngOnInit(): void {
    const dosageStreamCreator = (query) => this.dosageService.getList({...query , filter: this.filterText});
    this.list.maxResultCount = 10;
    this.list.hookToQuery(dosageStreamCreator).subscribe((response) => {
    this.dosage = response;
    });

    this.filterSubject$
      .pipe(
        debounceTime(800),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((filterValue) => {
        this.filterText = filterValue;
        this.list.get();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onFilterChange(value: string): void {
    this.filterSubject$.next(value);
  }

  clearSearch(): void {
    this.filterText = '';
    this.filterSubject$.next('');
  }
  
  createDosage(): void {
    this.selectedDosage = {} as DosageFormDto;
    this.buildForm(); 
    this.isDrawerOpen = true;
  }
  
  editDosage(id: string): void {
    this.dosageService.get(id).subscribe((res) => {
      this.selectedDosage = res;
      this.buildForm(); 
      this.isDrawerOpen = true;
    });
  }
  
  deleteDosage(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.dosageService.delete(id).subscribe(() => {
            this.list.get();
          });
        }
      });
    }

    buildForm(): void {
    this.form = this.fb.group({
      code: [this.selectedDosage.code || '', [Validators.required, Validators.maxLength(50)]],
      name: [this.selectedDosage.name || '', [Validators.required, Validators.maxLength(100)]],
    });

    //Logic sinh code khi tạo
    if (!this.selectedDosage.id) {
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

    const request = this.selectedDosage.id
      ? this.dosageService.update(this.selectedDosage.id, this.form.value)
      : this.dosageService.create(this.form.value);

    request.subscribe(() => {
      this.closeDrawer();
      this.list.get();
    });
  }
}
