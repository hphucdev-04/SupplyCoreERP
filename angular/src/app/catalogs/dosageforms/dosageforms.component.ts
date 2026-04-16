import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { DosageFormService } from 'src/app/proxy/dosage-forms';
import { DosageFormDto, GetDosageFormListDto } from 'src/app/proxy/dosage-forms/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer-component/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search-component/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  selector: 'app-dosageforms',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './dosageforms.component.html',
  styleUrl: './dosageforms.component.scss',
  providers: [ListService]
})
export class DosageformsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  dosage = { items: [], totalCount: 0 } as PagedResultDto<DosageFormDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedDosage = {} as DosageFormDto;
  filterText = '';

  constructor(
    public readonly list: ListService<GetDosageFormListDto>,
    private dosageService: DosageFormService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) { }

  ngOnInit(): void {
    const dosageStreamCreator = (query) => this.dosageService.getList({ ...query, filter: this.filterText });
    this.list.maxResultCount = 10;
    this.list.hookToQuery(dosageStreamCreator).subscribe((response) => {
      this.dosage = response;
    });
    this.buildForm()
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearch(searchValue: string): void {
    this.filterText = searchValue;
    this.list.get();
  }

  createDosage(): void {
    this.selectedDosage = {} as DosageFormDto;
    this.form.reset();
    this.isDrawerOpen = true;
  }


  editDosage(id: string): void {
    this.dosageService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedDosage = res;
        this.form.patchValue(res);
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

    const request = this.selectedDosage.id
      ? this.dosageService.update(this.selectedDosage.id, this.form.value)
      : this.dosageService.create(this.form.value);

    request.subscribe(() => {
      this.closeDrawer();
      this.list.get();
      this.toaster.success(
        this.selectedDosage.id ? '::UpdateSuccess' : '::CreateSuccess', ':Success'
      );
    });
  }
}
