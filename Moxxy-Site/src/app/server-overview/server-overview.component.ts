import { Component, OnInit } from '@angular/core';
import { MockServerData } from '../mock-server/mock-server-data';
import { MockServerService } from '../mock-server/mock-server.service';

@Component({
  selector: 'app-server-overview',
  templateUrl: './server-overview.component.html',
  styleUrls: ['./server-overview.component.css']
})
export class ServerOverviewComponent implements OnInit {
  Servers: MockServerData[];

  constructor(private mockServerService: MockServerService) { }

  toggleServer(server: MockServerData, index: number) {
    this.mockServerService.toggleServer(server).subscribe((newServer: MockServerData) => {
      this.Servers[index] = newServer;
    });
  }

  ngOnInit()
  {
    this.mockServerService.getMockServers().subscribe((servers: MockServerData[]) => {
      this.Servers = servers;
    });
  }

}
