import { ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { CategoryService } from 'src/app/proxy/categories';
import { CategoryDto, CreateUpdateCategoryDto, GetCategoryListDto } from 'src/app/proxy/categories/dtos';
import { DrawerComponent } from 'src/app/shared/components/drawer/drawer.component';
import { SearchComponent } from 'src/app/shared/components/search/search.component';
import { SharedModule } from 'src/app/shared/shared.module';

@Component({
  imports:[SharedModule, DrawerComponent, SearchComponent],
  selector: 'app-categories',
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss',
  providers: [ListService],
})
export class CategoriesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  category = { items: [], totalCount: 0 } as PagedResultDto<CategoryDto>;
  isDrawerOpen = false;
  form: FormGroup;
  selectedCategory = {} as CategoryDto;
  filterText = '';

  constructor(
    public readonly list: ListService<GetCategoryListDto>,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private confirmation: ConfirmationService,
    private toaster: ToasterService,
  ) {
    this.buildForm()
  }
 ngOnInit(): void {
    const categoryStreamCreator = (query: GetCategoryListDto) => this.categoryService.getList({...query, filter: this.filterText});
    this.list.maxResultCount = 10;
    this.list.hookToQuery(categoryStreamCreator).subscribe((response) => {
      this.category = response;
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

  createCategory(): void {
    this.selectedCategory = {} as CategoryDto;
    this.buildForm();
    this.isDrawerOpen = true;
  }

  editCategory(id: string): void {
    this.categoryService.get(id).subscribe((category) => {
      this.selectedCategory = category;
      this.buildForm();
      this.isDrawerOpen = true;
    });
  }

  buildForm(): void {
    this.form = this.fb.group({
      name: [
        this.selectedCategory.name || '', 
        [Validators.required, Validators.maxLength(100)] 
      ],
    });
  }

  closeDrawer(): void {
    this.isDrawerOpen = false;
    this.form.reset();
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }
    const payload = this.form.getRawValue() as CreateUpdateCategoryDto
    const request = this.selectedCategory.id
      ? this.categoryService.update(this.selectedCategory.id, payload)
      : this.categoryService.create(payload);

    request.subscribe(() => {
      this.closeDrawer();
      this.list.get();
      this.toaster.success(
        this.selectedCategory.id ? '::UpdateSuccess' : '::CreateSuccess','::Success'
      );
    });
  }

  deleteCategory(id: string): void {
    this.confirmation
      .warn('::AreYouSureToDelete', '::AreYouSure')
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.categoryService.delete(id).subscribe(() => {
            this.list.get();
            this.toaster.success('::DeleteSuccess', '::Success');
          });
        }
      });
  }
}
