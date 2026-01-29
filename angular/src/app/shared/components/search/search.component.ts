import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { debounceTime, distinctUntilChanged, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss'
})
export class SearchComponent implements OnInit, OnDestroy {
private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  @Input() placeholder: string = 'Tìm kiếm...';
  @Input() debounceTime: number = 500;
  @Input() value: string = '';
  
  @Output() valueChange = new EventEmitter<string>();
  @Output() search = new EventEmitter<string>();

  ngOnInit(): void {
    this.searchSubject$
      .pipe(
        debounceTime(this.debounceTime),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((searchValue) => {
        this.search.emit(searchValue);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onInputChange(value: string): void {
    this.value = value;
    this.valueChange.emit(value);
    this.searchSubject$.next(value);
  }

  clearSearch(): void {
    this.value = '';
    this.valueChange.emit('');
    this.searchSubject$.next('');
  }
}
