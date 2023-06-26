/*import React, { useEffect, useRef, useState } from "react";
import L from "leaflet";
import "leaflet.heat";
import axios from "axios";
import testData from '../test.json';

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

        const volgaData = testData.map(data => [data.Latitude, data.Longitude, data.Coefficient])


        if (currentCity === 'Волгоград') {
            leafletMap.setView([48.7071, 44.5169], 13);
        } else if (currentCity === 'Москва') {
            leafletMap.setView([55.751244, 37.618423], 13);
        } else if (currentCity === 'Санкт-Петербург') {
            leafletMap.setView([59.93863, 30.31413], 13);
        }

        setMap(leafletMap);

        setLocations(volgaData);
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
*/
import React, { useEffect, useRef, useState } from "react";
import L from "leaflet";
import "leaflet.heat";
import axios from "axios";
//import testData from '../test.csv';
//const json = '{"Center":{"Latitude":48.7308916,"Longitude":44.5232696},"Coefficient":1.0},{"Center":{"Latitude":48.7080165,"Longitude":44.5143223},"Coefficient":1.0},{"Center":{"Latitude":48.5149128,"Longitude":44.5350416},"Coefficient":1.0},{"Center":{"Latitude":48.5220994,"Longitude":44.5553595},"Coefficient":1.0},{"Center":{"Latitude":48.6955735,"Longitude":44.496278},"Coefficient":1.0},{"Center":{"Latitude":48.7122191,"Longitude":44.5242728},"Coefficient":1.0},{"Center":{"Latitude":48.7261122,"Longitude":44.5206049},"Coefficient":1.0},{"Center":{"Latitude":48.5075263,"Longitude":44.5813978},"Coefficient":1.0},{"Center":{"Latitude":48.5217777,"Longitude":44.6039533},"Coefficient":1.0},{"Center":{"Latitude":48.5180665,"Longitude":44.5670742},"Coefficient":1.0},{"Center":{"Latitude":48.6950814,"Longitude":44.4984234},"Coefficient":1.0},{"Center":{"Latitude":48.7054552,"Longitude":44.5183139},"Coefficient":1.0},{"Center":{"Latitude":48.7031222,"Longitude":44.5137392},"Coefficient":1.0},{"Center":{"Latitude":48.7129423,"Longitude":44.5249576},"Coefficient":1.0},{"Center":{"Latitude":48.7002509,"Longitude":44.5153308},"Coefficient":1.0},{"Center":{"Latitude":48.6956812,"Longitude":44.4694978},"Coefficient":1.0},{"Center":{"Latitude":48.5908287,"Longitude":44.3991505},"Coefficient":1.0},{"Center":{"Latitude":48.7089267,"Longitude":44.5158185},"Coefficient":1.0},{"Center":{"Latitude":48.7131717,"Longitude":44.5109574},"Coefficient":1.0},{"Center":{"Latitude":48.7079382,"Longitude":44.5255649},"Coefficient":1.0},{"Center":{"Latitude":48.7375007,"Longitude":44.5140843},"Coefficient":1.0},{"Center":{"Latitude":48.7135105,"Longitude":44.5050368},"Coefficient":1.0},{"Center":{"Latitude":48.7186595,"Longitude":44.5318317},"Coefficient":1.0},{"Center":{"Latitude":48.7054867,"Longitude":44.5202552},"Coefficient":1.0},{"Center":{"Latitude":48.7003975,"Longitude":44.518794},"Coefficient":1.0},{"Center":{"Latitude":48.7838162,"Longitude":44.5642236},"Coefficient":1.0},{"Center":{"Latitude":48.7524532,"Longitude":44.4961219},"Coefficient":1.0},{"Center":{"Latitude":48.7825997,"Longitude":44.5625622},"Coefficient":1.0},{"Center":{"Latitude":48.7054835,"Longitude":44.512666},"Coefficient":1.0},{"Center":{"Latitude":48.6893143,"Longitude":44.4921996},"Coefficient":1.0},{"Center":{"Latitude":48.7430741,"Longitude":44.5033584},"Coefficient":1.0},{"Center":{"Latitude":48.706824,"Longitude":44.5078608},"Coefficient":1.0},{"Center":{"Latitude":48.6972876,"Longitude":44.5009056},"Coefficient":1.0},{"Center":{"Latitude":48.7008599,"Longitude":44.519233},"Coefficient":1.0}';


const Heatmap = ({ currentCity, currentPlace, currentMethod }) => {

    const [map, setMap] = useState(null);
    const [locations, setLocations] = useState([]);

    function CSVToArray(strData, strDelimiter) {
        // Check to see if the delimiter is defined. If not,
        // then default to comma.
        strDelimiter = (strDelimiter || ",");

        // Create a regular expression to parse the CSV values.
        var objPattern = new RegExp(
            (
                // Delimiters.
                "(\\" + strDelimiter + "|\\r?\\n|\\r|^)" +

                // Quoted fields.
                "(?:\"([^\"]*(?:\"\"[^\"]*)*)\"|" +

                // Standard fields.
                "([^\"\\" + strDelimiter + "\\r\\n]*))"
            ),
            "gi"
        );


        // Create an array to hold our data. Give the array
        // a default empty first row.
        var arrData = [[]];

        // Create an array to hold our individual pattern
        // matching groups.
        var arrMatches = null;


        // Keep looping over the regular expression matches
        // until we can no longer find a match.
        while (arrMatches = objPattern.exec(strData)) {

            // Get the delimiter that was found.
            var strMatchedDelimiter = arrMatches[1];

            // Check to see if the given delimiter has a length
            // (is not the start of string) and if it matches
            // field delimiter. If id does not, then we know
            // that this delimiter is a row delimiter.
            if (
                strMatchedDelimiter.length &&
                (strMatchedDelimiter != strDelimiter)
            ) {

                // Since we have reached a new row of data,
                // add an empty row to our data array.
                arrData.push([]);

            }


            // Now that we have our delimiter out of the way,
            // let's check to see which kind of value we
            // captured (quoted or unquoted).
            if (arrMatches[2]) {

                // We found a quoted value. When we capture
                // this value, unescape any double quotes.
                var strMatchedValue = arrMatches[2].replace(
                    new RegExp("\"\"", "g"),
                    "\""
                );

            } else {

                // We found a non-quoted value.
                var strMatchedValue = arrMatches[3];

            }


            // Now that we have our value string, let's add
            // it to the data array.
            arrData[arrData.length - 1].push(strMatchedValue);
        }

        // Return the parsed data.
        return (arrData);
    }

    const mapRef = useRef();

    const fetchData = async () => {
        return await axios.get('')
    }
    function jsonToObjArray(json) {
        const arr = JSON.parse(json);
        const objArr = [];

        for (const item of arr) {
            const center = item.Center;
            const obj = {
                Latitude: center.Latitude,
                Longitude: center.Longitude,
                Coefficient: item.Coefficient
            };

            objArr.push(obj);
        }

        return objArr;
    }
    useEffect(() => {
        let locations = [];
        let leafletMap = L.map(mapRef.current)
        //const volgaData = testData.map(data => [data.Center.Latitude, data.Center.Longitude, data.Coefficient])
        //const obj = JSON.parse(testData);
        //const volgaData = jsonToObjArray(json);
        const volgaDataTest = [
            { Latitude: 48.7308916, Longitude: 44.5232696, Coefficient: 1 },
            { Latitude: 48.7080165, Longitude: 44.5143223, Coefficient: 1 },
            { Latitude: 48.5149128, Longitude: 44.5350416, Coefficient: 1 },
            { Latitude: 48.5220994, Longitude: 44.5553595, Coefficient: 1 },
            { Latitude: 48.6955735, Longitude: 44.496278, Coefficient: 1 },
            { Latitude: 48.7122191, Longitude: 44.5242728, Coefficient: 1 },
            { Latitude: 48.7261122, Longitude: 44.5206049, Coefficient: 1 },
            { Latitude: 48.5075263, Longitude: 44.5813978, Coefficient: 1 },
            { Latitude: 48.5217777, Longitude: 44.6039533, Coefficient: 1 },
            { Latitude: 48.5180665, Longitude: 44.5670742, Coefficient: 1 },
            { Latitude: 48.6950814, Longitude: 44.4984234, Coefficient: 1 },
            { Latitude: 48.7054552, Longitude: 44.5183139, Coefficient: 1 },
            { Latitude: 48.7031222, Longitude: 44.5137392, Coefficient: 1 },
            { Latitude: 48.7129423, Longitude: 44.5249576, Coefficient: 1 },
            { Latitude: 48.7002509, Longitude: 44.5153308, Coefficient: 1 },
            { Latitude: 48.6956812, Longitude: 44.4694978, Coefficient: 1 },
            { Latitude: 48.5908287, Longitude: 44.3991505, Coefficient: 1 },
            { Latitude: 48.7089267, Longitude: 44.5158185, Coefficient: 1 },
            { Latitude: 48.7131717, Longitude: 44.5109574, Coefficient: 1 },
            { Latitude: 48.7079382, Longitude: 44.5255649, Coefficient: 1 },
            { Latitude: 48.7375007, Longitude: 44.5140843, Coefficient: 1 },
            { Latitude: 48.7135105, Longitude: 44.5050368, Coefficient: 1 },
            { Latitude: 48.7186595, Longitude: 44.5318317, Coefficient: 1 },
            { Latitude: 48.7054867, Longitude: 44.5202552, Coefficient: 1 },
            { Latitude: 48.7003975, Longitude: 44.518794, Coefficient: 1 },
            { Latitude: 48.7838162, Longitude: 44.5642236, Coefficient: 1 },
            { Latitude: 48.7524532, Longitude: 44.4961219, Coefficient: 1 },
            { Latitude: 48.7825997, Longitude: 44.5625622, Coefficient: 1 },
            { Latitude: 48.7054835, Longitude: 44.512666, Coefficient: 1 },
            { Latitude: 48.6893143, Longitude: 44.4921996, Coefficient: 1 },
            { Latitude: 48.7430741, Longitude: 44.5033584, Coefficient: 1 },
            { Latitude: 48.706824, Longitude: 44.5078608, Coefficient: 1 },
            { Latitude: 48.6972876, Longitude: 44.5009056, Coefficient: 1 },
            { Latitude: 48.7008599, Longitude: 44.519233, Coefficient: 1 }
        ];
        const volgaData = volgaDataTest.map(obj => Object.values(obj));//CSVToArray(testData,",");

        if (currentCity === 'Волгоград') {
            leafletMap.setView([48.7071, 44.5169], 13);
            locations = volgaData
        } else if (currentCity === 'Москва') {
            leafletMap.setView([55.751244, 37.618423], 13);
            locations = [
                [55.751244, 37.618423, 1],
                [55.756456, 37.617082, 0.7],
                [55.749611, 37.580338, 0.5],
                [55.763079, 37.617332, 0.4],
                [55.753714, 37.621298, 0.2],
                [55.767831, 37.638138, 0.1],
                // и т.д.
            ];
        } else if (currentCity === 'Санкт-Петербург') {
            leafletMap.setView([59.93863, 30.31413], 13);
            locations = [
                [59.93863, 30.31413, 1],
                [59.93608, 30.31593, 0.7],
                [59.94381, 30.35402, 0.5],
                [59.94433, 30.36114, 0.4],
                [59.93153, 30.36051, 0.2],
                [59.95633, 30.31434, 0.1],
                // и т.д.
            ];
        }

        setMap(leafletMap);

        setLocations(locations);
        const heatLayer = L.heatLayer(locations, { radius: 30, blur: 30, maxZoom: 15, minZoom: 5 }).addTo(leafletMap);

        // Добавляем базовый слой карты
        const osmUrl = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
        const osmAttrib = 'Map data © <a href="https://openstreetmap.org">OpenStreetMap</a> contributors';
        const osm = new L.TileLayer(osmUrl, {
            attribution: osmAttrib,
            minZoom: 8,
            maxZoom: 16,
        });
        osm.addTo(leafletMap);

        // Очищаем карту при размонтировании компонента
        return () => {
            leafletMap.remove();
        };
    }, [currentCity, currentPlace, currentMethod]);

    return <div ref={mapRef}></div>;
};

export default Heatmap;