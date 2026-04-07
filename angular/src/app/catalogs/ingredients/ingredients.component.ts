import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { ActiveIngredientService } from 'src/app/proxy/active-ingredients';
import { ActiveIngredientDto, GetActiveIngredientListDto } from 'src/app/proxy/active-ingredients/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { CodeGeneratorUtil } from 'src/app/shared/utils/code-generator.util';

@Component({
  selector: 'app-ingredient',
  imports: [SharedModule, DrawerComponent, SearchComponent],
  templateUrl: './ingredients.component.html',
  styleUrl: './ingredients.component.scss',
  providers: [ListService]
})
export class IngredientsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  ingredient = {items: [], totalCount: 0} as PagedResultDto<ActiveIngredientDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedIngredient = {} as ActiveIngredientDto;
  filterText = '';

  constructor(
    public readonly list: ListService<GetActiveIngredientListDto>,
    private ingredientService: ActiveIngredientService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ){}

  ngOnInit(): void {
      const ingredientStreamCreator = (query) => this.ingredientService.getList({...query, filter: this.filterText});
      this.list.maxResultCount = 10;
      this.list.hookToQuery(ingredientStreamCreator).subscribe((response) => {
        this.ingredient = response;
      })
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

  createIngredient(): void {
    this.selectedIngredient = {} as ActiveIngredientDto;
    this.form.reset();
    this.isDrawerOpen = true;
  }

  editIngredient(id: string): void {
    this.ingredientService.get(id)
      .pipe(takeUntil(this.destroy$))
      .subscribe((res) => {
        this.selectedIngredient = res;
        this.form.patchValue(res);
        this.isDrawerOpen = true;
      });
  }

  deleteIngredient(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.ingredientService.delete(id).subscribe(() => {
            this.list.get();
            this.toaster.success('::DeleteSuccess', '::Success')
        });
      }
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      name: ['', [Validators.required, Validators.maxLength(255)]],
    });
  }
 generateCode(): void {
    const name = this.form.get('name')?.value || '';
    const code = CodeGeneratorUtil.generate(name, 'AI');
    this.form.get('code')?.setValue(code);
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }
   save(): void {
    if (this.form.invalid) return;

    const request = this.selectedIngredient.id
      ? this.ingredientService.update(this.selectedIngredient.id, this.form.value)
      : this.ingredientService.create(this.form.value);

    request.subscribe(() => {
      this.closeDrawer();
      this.list.get();
      this.toaster.success(
        this.selectedIngredient.id ? '::UpdateSuccess' : 'CreateSuccess', '::Success'
      );
    });
  }
}
