import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

interface Item {
  id: number;
  name: string;
  description?: string;
  createdAt: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="max-width:600px;margin:40px auto;padding:0 16px;">
      <h1>Mini TWISE</h1>
      <p>Angular frontend &rarr; Nginx &rarr; ASP.NET Core API &rarr; PostgreSQL</p>

      <div style="display:flex;gap:8px;margin-bottom:16px;">
        <input [(ngModel)]="newName" placeholder="Item name" style="flex:1;padding:8px;" />
        <button (click)="addItem()" style="padding:8px 16px;">Add</button>
      </div>

      <ul style="list-style:none;padding:0;">
        <li *ngFor="let item of items()" style="padding:8px;border-bottom:1px solid #ddd;display:flex;justify-content:space-between;">
          <span>{{ item.name }}</span>
          <button (click)="deleteItem(item.id)" style="color:#c0392b;background:none;border:none;cursor:pointer;">Delete</button>
        </li>
      </ul>

      <p *ngIf="error()" style="color:#c0392b;">{{ error() }}</p>
    </div>
  `
})
export class AppComponent implements OnInit {
  items = signal<Item[]>([]);
  error = signal<string | null>(null);
  newName = '';

  // In Docker, Nginx proxies /api to the API container, so relative path works
  private apiUrl = '/api/items';

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems() {
    this.http.get<Item[]>(this.apiUrl).subscribe({
      next: (data) => this.items.set(data),
      error: () => this.error.set('Could not reach API')
    });
  }

  addItem() {
    if (!this.newName.trim()) return;
    this.http.post<Item>(this.apiUrl, { name: this.newName }).subscribe({
      next: () => {
        this.newName = '';
        this.loadItems();
      },
      error: () => this.error.set('Could not create item')
    });
  }

  deleteItem(id: number) {
    this.http.delete(`${this.apiUrl}/${id}`).subscribe({
      next: () => this.loadItems(),
      error: () => this.error.set('Could not delete item')
    });
  }
}
