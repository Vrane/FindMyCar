namespace FindMyCar
{
    using System;
    using System.Device.Location;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Shapes;

    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Controls.Maps;
    using Microsoft.Phone.Controls.Maps.Platform;

    using sysLoc = System.Device.Location;

    public partial class MainPage : PhoneApplicationPage
    {
        // This is the global internal variable where results are stored. These are accessed later to calculate the route.
        internal geocodeservice.GeocodeResult[] GeocodeResults;

        private Location _location = new Location();

        private sysLoc::GeoCoordinateWatcher _geoCoordinateWatcher;

        private GeoCoordinateWatcher _newGeoCoordinateWatcher;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            map.LogoVisibility = Visibility.Collapsed;
            map.CopyrightVisibility = Visibility.Collapsed;
        }

        private void GeoCoordinateWatcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown)
            {
                MessageBox.Show("Please wait while your prosition is determined....");
            }

            map.Center = new sysLoc::GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);

            if (map.Children.Count != 0)
            {
                var pushPin = map.Children.FirstOrDefault(p => (p is Pushpin && ((Pushpin)p).Tag == "locationPushpin"));

                if (pushPin != null)
                {
                    map.Children.Remove(pushPin);
                }
            }

            Pushpin locationPushpin = new Pushpin
                                          {
                                              Tag = "locationPushpin",
                                              Location = _geoCoordinateWatcher.Position.Location,
                                              Background = new SolidColorBrush(Colors.Purple),
                                              Content = "You are here " + _geoCoordinateWatcher.Position.Location
                                          };
            map.Children.Add(locationPushpin);
            map.SetView(_geoCoordinateWatcher.Position.Location, 18.0);

        }

        private void GeoCoordinateWatcherStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case sysLoc::GeoPositionStatus.Disabled:
                    MessageBox.Show("Location service is not enabled");
                    break;
                case sysLoc::GeoPositionStatus.NoData:
                    MessageBox.Show("The Location Service is working, but it cannot get _location data ");
                    break;
            }
        }

        private void LocateApplicationBarIconButtonOnClick(object sender, EventArgs e)
        {
            if (_geoCoordinateWatcher == null)
            {
                _geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High) { MovementThreshold = 20 };
                _geoCoordinateWatcher.StatusChanged += GeoCoordinateWatcherStatusChanged;
                _geoCoordinateWatcher.PositionChanged += GeoCoordinateWatcherPositionChanged;
            }
            _geoCoordinateWatcher.Start();
        }

        private void FindNewLocationApplicationBarIconButtonOnClick(object sender, EventArgs e)
        {
            this.FindMyNewLocation();
        }

        private void SaveApplicationBarIconButtonOnClick(object sender, EventArgs e)
        {
            var savedLocation = _geoCoordinateWatcher.Position.Location;
            TextBoxForLocation.Text = savedLocation.ToString();
        }

        private GeoCoordinateWatcher FindMyNewLocation()
        {           
            if (this._newGeoCoordinateWatcher == null)
            {
                this._newGeoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High) { MovementThreshold = 20 };
                this._newGeoCoordinateWatcher.StatusChanged += NewGeoCoordinateWatcherStatusChanged;
                this._newGeoCoordinateWatcher.PositionChanged += NewGeoCoordinateWatcherPositionChanged;
            }

            this._newGeoCoordinateWatcher.Start();

            return this._newGeoCoordinateWatcher;
        }

        private void NewGeoCoordinateWatcherPositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (e.Position.Location.IsUnknown)
            {
                MessageBox.Show("Please wait while your prosition is determined....");
            }

            map.Center = new sysLoc::GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);

            if (map.Children.Count != 0)
            {
                var pushPin = map.Children.FirstOrDefault(p => (p is Pushpin && ((Pushpin)p).Tag == "locationPushpin"));

                if (pushPin != null)
                {
                    map.Children.Remove(pushPin);
                }
            }

            Pushpin locationPushpin = new Pushpin
            {
                Tag = "locationPushpin",
                Location = this._newGeoCoordinateWatcher.Position.Location,
                Background = new SolidColorBrush(Colors.Purple),
                Content = "You are here " + this._newGeoCoordinateWatcher.Position.Location
            };
            map.Children.Add(locationPushpin);
            map.SetView(this._newGeoCoordinateWatcher.Position.Location, 18.0);
        }

        private void FindMyCarApplicationBarIconButtonOnClick(object sender, EventArgs e)
        {
            this.GeocodeResults = new geocodeservice.GeocodeResult[2];

            this.Geocode(this.TextBoxForLocation.Text, 0);
            var myNewSavedLocation = this._newGeoCoordinateWatcher.Position.Location;
            TextBoxForNewLocation.Text = myNewSavedLocation.ToString();
            this.Geocode(this.TextBoxForNewLocation.Text, 1);
        }

        private void NewGeoCoordinateWatcherStatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case sysLoc::GeoPositionStatus.Disabled:
                    MessageBox.Show("Location service is not enabled");
                    break;
                case sysLoc::GeoPositionStatus.NoData:
                    MessageBox.Show("The Location Service is working, but it cannot get _location data ");
                    break;
            }
        }

        private void Geocode(string strAddress, int waypointIndex)
        {
            // Create the service variable and set the callback method using the GeocodeCompleted property.
            geocodeservice.GeocodeServiceClient geocodeService =
                new geocodeservice.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            geocodeService.GeocodeCompleted +=
                new EventHandler<geocodeservice.GeocodeCompletedEventArgs>(geocodeService_GeocodeCompleted);

            // Set the credentials and the geocode query, which could be an address or _location.
            geocodeservice.GeocodeRequest geocodeRequest = new geocodeservice.GeocodeRequest();
            geocodeRequest.Credentials = new Credentials();
            geocodeRequest.Credentials.ApplicationId =
                ((ApplicationIdCredentialsProvider)map.CredentialsProvider).ApplicationId;
            geocodeRequest.Query = strAddress;

            // Make the asynchronous Geocode request, using the ‘waypoint index’ as 
            //   the user state to track this request and allow it to be identified when the response is returned.
            geocodeService.GeocodeAsync(geocodeRequest, waypointIndex);
        }

        // This is the Geocode request callback method.
        private void geocodeService_GeocodeCompleted(object sender, geocodeservice.GeocodeCompletedEventArgs e)
        {
            // Retrieve the user state of this response (the ‘waypoint index’) to identify which geocode request 
            //   it corresponds to.
            int waypointIndex = System.Convert.ToInt32(e.UserState);

            // Retrieve the GeocodeResult for this response and store it in the global variable geocodeResults, using
            //   the waypoint index to position it in the array.
            this.GeocodeResults[waypointIndex] = e.Result.Results[0];

            // Look at each element in the global gecodeResults array to figure out if more geocode responses still 
            //   need to be returned.

            bool doneGeocoding = true;

            foreach (geocodeservice.GeocodeResult gr in this.GeocodeResults)
            {
                if (gr == null)
                {
                    doneGeocoding = false;
                }
            }

            // If the geocodeResults array is totally filled, then calculate the route.
            if (doneGeocoding) CalculateRoute(this.GeocodeResults);
        }

        private void CalculateRoute(geocodeservice.GeocodeResult[] results)
        {
            // Create the service variable and set the callback method using the CalculateRouteCompleted property.
            routeservice.RouteServiceClient routeService =
                new routeservice.RouteServiceClient("BasicHttpBinding_IRouteService");
            routeService.CalculateRouteCompleted +=
                new EventHandler<routeservice.CalculateRouteCompletedEventArgs>(routeService_CalculateRouteCompleted);

            // Set the token.
            routeservice.RouteRequest routeRequest = new routeservice.RouteRequest();
            routeRequest.Credentials = new Credentials();
            routeRequest.Credentials.ApplicationId = "ApgLkoHIG4rNShRJAxMMNettsv6SWs3eP8OchozFS89Vex7BRHsSbCr31HkvYK-d";

            // Return the route points so the route can be drawn.
            routeRequest.Options = new routeservice.RouteOptions();
            routeRequest.Options.RoutePathType = routeservice.RoutePathType.Points;

            // Set the waypoints of the route to be calculated using the Geocode Service results stored in the geocodeResults variable.
            routeRequest.Waypoints = new System.Collections.ObjectModel.ObservableCollection<routeservice.Waypoint>();
            foreach (geocodeservice.GeocodeResult result in results)
            {
                routeRequest.Waypoints.Add(GeocodeResultToWaypoint(result));
            }

            // Make the CalculateRoute asnychronous request.
            routeService.CalculateRouteAsync(routeRequest);
        }

        private routeservice.Waypoint GeocodeResultToWaypoint(geocodeservice.GeocodeResult result)
        {
            routeservice.Waypoint waypoint = new routeservice.Waypoint();
            waypoint.Description = result.DisplayName;
            waypoint.Location = new Location();
            waypoint.Location.Latitude = result.Locations[0].Latitude;
            waypoint.Location.Longitude = result.Locations[0].Longitude;
            return waypoint;
        }

        private void routeService_CalculateRouteCompleted(object sender, routeservice.CalculateRouteCompletedEventArgs e)
        {
            // If the route calculate was a success and contains a route, then draw the route on the map.
            if ((e.Result.ResponseSummary.StatusCode == routeservice.ResponseStatusCode.Success)
                & (e.Result.Result.Legs.Count != 0))
            {
                // Set properties of the route line you want to draw.
                Color routeColor = Colors.Blue;
                SolidColorBrush routeBrush = new SolidColorBrush(routeColor);
                MapPolyline routeLine = new MapPolyline
                                            {
                                                Locations = new LocationCollection(),
                                                Stroke = routeBrush,
                                                Opacity = 0.65,
                                                StrokeThickness = 5.0
                                            };

                // Retrieve the route points that define the shape of the route.
                foreach (Location p in e.Result.Result.RoutePath.Points)
                {
                    routeLine.Locations.Add(new Location { Latitude = p.Latitude, Longitude = p.Longitude });

                }

                // Add a map layer in which to draw the route.
                MapLayer myRouteLayer = new MapLayer();
                map.Children.Add(myRouteLayer);

                // Add the route line to the new layer.
                myRouteLayer.Children.Add(routeLine);

                // Figure the rectangle which encompasses the route. This is used later to set the map view.
                double centerlatitude = (routeLine.Locations[0].Latitude
                                         + routeLine.Locations[routeLine.Locations.Count - 1].Latitude) / 2;

                double centerlongitude = (routeLine.Locations[0].Longitude
                                          + routeLine.Locations[routeLine.Locations.Count - 1].Longitude) / 2;

                Location centerloc = new Location
                                         {
                                             Latitude = centerlatitude, 
                                             Longitude = centerlongitude
                                         };

                double north, south, east, west;

                if ((routeLine.Locations[0].Latitude > 0)
                    && (routeLine.Locations[routeLine.Locations.Count - 1].Latitude > 0))
                {
                    north = routeLine.Locations[0].Latitude
                            > routeLine.Locations[routeLine.Locations.Count - 1].Latitude
                                ? routeLine.Locations[0].Latitude
                                : routeLine.Locations[routeLine.Locations.Count - 1].Latitude;
                    south = routeLine.Locations[0].Latitude
                            < routeLine.Locations[routeLine.Locations.Count - 1].Latitude
                                ? routeLine.Locations[0].Latitude
                                : routeLine.Locations[routeLine.Locations.Count - 1].Latitude;
                }
                else
                {
                    north = routeLine.Locations[0].Latitude
                            < routeLine.Locations[routeLine.Locations.Count - 1].Latitude
                                ? routeLine.Locations[0].Latitude
                                : routeLine.Locations[routeLine.Locations.Count - 1].Latitude;
                    south = routeLine.Locations[0].Latitude
                            > routeLine.Locations[routeLine.Locations.Count - 1].Latitude
                                ? routeLine.Locations[0].Latitude
                                : routeLine.Locations[routeLine.Locations.Count - 1].Latitude;

                }
                if ((routeLine.Locations[0].Longitude < 0)
                    && (routeLine.Locations[routeLine.Locations.Count - 1].Longitude < 0))
                {
                    west = routeLine.Locations[0].Longitude
                           < routeLine.Locations[routeLine.Locations.Count - 1].Longitude
                               ? routeLine.Locations[0].Longitude
                               : routeLine.Locations[routeLine.Locations.Count - 1].Longitude;
                    east = routeLine.Locations[0].Longitude
                           > routeLine.Locations[routeLine.Locations.Count - 1].Longitude
                               ? routeLine.Locations[0].Longitude
                               : routeLine.Locations[routeLine.Locations.Count - 1].Longitude;
                }
                else
                {
                    west = routeLine.Locations[0].Longitude
                           > routeLine.Locations[routeLine.Locations.Count - 1].Longitude
                               ? routeLine.Locations[0].Longitude
                               : routeLine.Locations[routeLine.Locations.Count - 1].Longitude;
                    east = routeLine.Locations[0].Longitude
                           < routeLine.Locations[routeLine.Locations.Count - 1].Longitude
                               ? routeLine.Locations[0].Longitude
                               : routeLine.Locations[routeLine.Locations.Count - 1].Longitude;
                }

                // For each geocode result (which are the waypoints of the route), draw a dot on the map.
                foreach (geocodeservice.GeocodeResult gr in this.GeocodeResults)
                {
                    Ellipse point = new Ellipse();
                    point.Width = 10;
                    point.Height = 10;
                    point.Fill = new SolidColorBrush(Colors.Red);
                    point.Opacity = 0.65;
                    this._location.Latitude = gr.Locations[0].Latitude;
                    this._location.Longitude = gr.Locations[0].Longitude;
                    MapLayer.SetPosition(point, this._location);
                    MapLayer.SetPositionOrigin(point, PositionOrigin.Center);

                    // Add the drawn point to the route layer.                    
                    myRouteLayer.Children.Add(point);
                }

                // Set the map view using the rectangle which bounds the rendered route.

                //map1.SetView(rect);
                double latitude = 0.0;
                double longtitude = 0.0;
                map.SetView(this._location, 12);
                map.Center = this._location;
                GeoCoordinate CurrentLocCoordinate = new System.Device.Location.GeoCoordinate(latitude, longtitude);
            }
        }

    }
}