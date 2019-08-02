import { Injectable } from '@angular/core';
import { MockServerData } from './mock-server-data';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { stringify } from '@angular/compiler/src/util';

@Injectable({
  providedIn: 'root'
})
export class MockServerService
{
  url = 'http://localhost:53348';
  controller = '/api/mockservers';
  constructor(private http: HttpClient) { }

  getMockServers() {
    return this.http.get(this.url + this.controller);
  };

  toggleServer(server: MockServerData) {
    let headers = new HttpHeaders();
    headers = headers.append('Content-Type', 'application/json');
    return this.http.put(
      this.url + this.controller + '/' + encodeURI(server.name) + '/activation',
      (!server.active).toString(), {
        headers: headers
      }
    );
  }

  promotePassthroughRoutes(server: MockServerData) {
    return this.http.get(this.url + this.controller + '/' + encodeURI(server.name) + '/PromoteRoutes');
  }

}
