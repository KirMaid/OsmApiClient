import React, { useEffect, useRef, useState } from "react";
import L from "leaflet";
import "leaflet.heat";
import axios from "axios";
//import testData from '../test.json';

const Heatmap = ({currentCity, currentPlace, currentMethod}) => {

    const [map, setMap] = useState(null);
    const [locations, setLocations] = useState([]);

    const mapRef = useRef();

    const fetchData = async () => {
        return await axios.get('')
    }

    useEffect(() => {
        let locations = [];
        let leafletMap = L.map(mapRef.current)

        axios.get('https://localhost:44389/api/getmap').then((resp) => {
            console.log(resp.data);
            //const json = JSON.parse(resp.data);
            //const heatmap = json.data.map(data => [data.Latitude, data.Longitude, data.Coefficient]);
            //setLocations(heatmap);
        });

        //const volgaData = testData.map(data => [data.Latitude, data.Longitude, data.Coefficient])


        if (currentCity === 'Волгоград') {
            leafletMap.setView([48.7071, 44.5169], 13);
        } else if (currentCity === 'Москва') {
            leafletMap.setView([55.751244, 37.618423], 13);
        } else if (currentCity === 'Санкт-Петербург') {
            leafletMap.setView([59.93863, 30.31413], 13);
        }

        setMap(leafletMap);

        setLocations(locations);
        const heatLayer = L.heatLayer(locations, { radius: 10, blur: 20, maxZoom: 11, minZoom: 0 }).addTo(leafletMap);

        // Добавляем базовый слой карты
        const osmUrl = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
        const osmAttrib = 'Map data © <a href="https://openstreetmap.org">OpenStreetMap</a> contributors';
        const osm = new L.TileLayer(osmUrl, {
            attribution: osmAttrib,
            minZoom: 8,
            maxZoom: 16,
        });

        // Очищаем карту при размонтировании компонента
        return () => {
            leafletMap.remove();
        };
    }, [currentCity, currentPlace, currentMethod]);

    return <div ref={mapRef}></div>;
};

export default Heatmap;