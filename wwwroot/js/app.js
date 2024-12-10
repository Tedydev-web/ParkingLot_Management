const GOONG_API_KEY = 'YJTajS80fLlhJ4a2BG0gXqXXdZzdLG5V3iivOK9e';
const GOONG_API_ACCESS = 'WlmIT8XtdGdBS6pBOePEve49zUx9waRQDSOXrVRv';
const API_URL = 'http://localhost:5257/api';

// Khởi tạo map
const map = new goongjs.Map({
	container: 'map',
	style: 'https://tiles.goong.io/assets/goong_map_web.json',
	center: [105.83991, 21.028], // Hà Nội
	zoom: 12,
	accessToken: GOONG_API_KEY,
});

// Thêm biến để lưu marker tạm thời
let tempMarker = null;

// Thêm biến và hàm mới vào đầu file
let directionsLine = null;

// Thêm biến để lưu vị trí hiện tại
let currentLocationSource = null;
let currentLocationLayer = null;

// Thêm biến để lưu token
let accessToken = localStorage.getItem('accessToken');
let refreshToken = localStorage.getItem('refreshToken');

// Thêm hiệu ứng chấm đập
const pulsingDot = {
	width: 200,
	height: 200,
	data: new Uint8Array(200 * 200 * 4),

	onAdd: function () {
		const canvas = document.createElement('canvas');
		canvas.width = this.width;
		canvas.height = this.height;
		this.context = canvas.getContext('2d');
	},

	render: function () {
		const duration = 1000;
		const t = (performance.now() % duration) / duration;

		const radius = (this.width / 2) * 0.3;
		const outerRadius = (this.width / 2) * 0.7 * t + radius;
		const context = this.context;

		context.clearRect(0, 0, this.width, this.height);

		// Vẽ vòng tròn ngoài
		context.beginPath();
		context.arc(this.width / 2, this.height / 2, outerRadius, 0, Math.PI * 2);
		context.fillStyle = `rgba(66, 135, 245, ${1 - t})`;
		context.fill();

		// Vẽ vòng tròn trong
		context.beginPath();
		context.arc(this.width / 2, this.height / 2, radius, 0, Math.PI * 2);
		context.fillStyle = 'rgba(66, 135, 245, 1)';
		context.strokeStyle = 'white';
		context.lineWidth = 2 + 4 * (1 - t);
		context.fill();
		context.stroke();

		this.data = context.getImageData(0, 0, this.width, this.height).data;
		map.triggerRepaint();
		return true;
	},
};

// Thêm sự kiện khi map load xong
map.on('load', function () {
	map.addImage('pulsing-dot', pulsingDot, { pixelRatio: 2 });

	// Khởi tạo source và layer cho vị trí hiện tại
	currentLocationSource = map.addSource('current-location', {
		type: 'geojson',
		data: {
			type: 'FeatureCollection',
			features: [
				{
					type: 'Feature',
					geometry: {
						type: 'Point',
						coordinates: [0, 0],
					},
				},
			],
		},
	});

	currentLocationLayer = map.addLayer({
		id: 'current-location',
		type: 'symbol',
		source: 'current-location',
		layout: {
			'icon-image': 'pulsing-dot',
		},
	});

	// Cập nhật vị trí hiện tại
	if ('geolocation' in navigator) {
		navigator.geolocation.watchPosition((position) => {
			const { longitude, latitude } = position.coords;
			map.getSource('current-location').setData({
				type: 'FeatureCollection',
				features: [
					{
						type: 'Feature',
						geometry: {
							type: 'Point',
							coordinates: [longitude, latitude],
						},
					},
				],
			});

			// Di chuyển map đến vị trí hiện tại
			map.flyTo({
				center: [longitude, latitude],
				zoom: 15,
			});
		});
	}
});

// Thêm hàm lấy địa chỉ từ tọa độ
async function getAddressFromCoordinates(lat, lng) {
	try {
		const response = await fetch(`https://rsapi.goong.io/Geocode?latlng=${lat},${lng}&api_key=${GOONG_API_ACCESS}`);

		if (response.ok) {
			const data = await response.json();
			console.log('Goong API Response:', data);

			if (data.status === 'OK' && data.results && data.results.length > 0) {
				const result = data.results[0];
				if (result.formatted_address) {
					return result.formatted_address;
				}
			}
		}
	} catch (error) {
		console.error('Error getting address:', error);
	}
	return '';
}

// Cập nhật sự kiện click map
map.on('click', async (e) => {
	const lngLat = e.lngLat;

	// Cập nhật form coordinates
	document.getElementById('latitude').value = lngLat.lat;
	document.getElementById('longitude').value = lngLat.lng;

	// Lấy và cập nhật địa chỉ tự động
	const address = await getAddressFromCoordinates(lngLat.lat, lngLat.lng);
	if (address) {
		document.getElementById('address').value = address;
	}

	// Xóa marker tạm thời cũ nếu có
	if (tempMarker) {
		tempMarker.remove();
	}

	// Tạo marker tạm thời mới
	tempMarker = new goongjs.Marker({
		color: '#FF0000',
		draggable: true,
	})
		.setLngLat(lngLat)
		.addTo(map);

	// Thêm sự kiện khi kéo thả marker
	tempMarker.on('dragend', async () => {
		const newLngLat = tempMarker.getLngLat();
		document.getElementById('latitude').value = newLngLat.lat;
		document.getElementById('longitude').value = newLngLat.lng;

		// Cập nhật địa chỉ khi kéo thả marker
		const newAddress = await getAddressFromCoordinates(newLngLat.lat, newLngLat.lng);
		if (newAddress) {
			document.getElementById('address').value = newAddress;
		}
	});
});

// Load danh sách bãi đỗ xe
async function loadParkingLots() {
	try {
		const response = await fetchWithAuth(`${API_URL}/ParkingLots`);
		const parkingLots = await response.json();

		// Xóa markers cũ
		document.querySelectorAll('.marker').forEach((marker) => marker.remove());

		// Hiển thị danh sách
		const listElement = document.getElementById('parkingLotList');
		listElement.innerHTML = parkingLots
			.map(
				(lot) => `
            <div class="parking-lot-item">
                <h3>${lot.name}</h3>
                <p>${lot.address}</p>
                <p>Chỗ trống: ${lot.availableSpots}/${lot.capacity}</p>
                <button class="get-directions-btn" 
                        onclick="showDirections(${lot.latitude}, ${lot.longitude})">
                    Chỉ đường
                </button>
                <button class="delete-btn" onclick="deleteParkingLot(${lot.id})">Xóa</button>
            </div>
        `
			)
			.join('');

		// Thêm markers
		parkingLots.forEach((lot) => {
			new goongjs.Marker({
				color: lot.markerColor,
			})
				.setLngLat([lot.longitude, lot.latitude])
				.setPopup(
					new goongjs.Popup().setHTML(`
                    <h3>${lot.name}</h3>
                    <p>${lot.address}</p>
                    <p>Chỗ trống: ${lot.availableSpots}/${lot.capacity}</p>
                `)
				)
				.addTo(map);
		});
	} catch (error) {
		console.error('Error:', error);
	}
}

// Cập nhật hàm submit form
document.getElementById('parkingLotForm').addEventListener('submit', async (e) => {
	e.preventDefault();

	const data = {
		name: document.getElementById('name').value,
		address: document.getElementById('address').value,
		latitude: parseFloat(document.getElementById('latitude').value),
		longitude: parseFloat(document.getElementById('longitude').value),
		capacity: parseInt(document.getElementById('capacity').value),
		description: document.getElementById('description').value,
	};

	try {
		const response = await fetchWithAuth(`${API_URL}/ParkingLots`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(data),
		});

		if (response.ok) {
			e.target.reset();
			if (tempMarker) {
				tempMarker.remove();
				tempMarker = null;
			}
			loadParkingLots();
		} else {
			const error = await response.json();
			alert(error.message || 'Không thể thêm bãi đỗ xe');
		}
	} catch (error) {
		console.error('Error:', error);
		alert('Đã xảy ra lỗi khi thêm bãi đỗ xe');
	}
});

// Xóa bãi đỗ xe
async function deleteParkingLot(id) {
	try {
		await fetchWithAuth(`${API_URL}/ParkingLots/${id}`, {
			method: 'DELETE',
		});
		loadParkingLots();
	} catch (error) {
		console.error('Error:', error);
		alert('Không thể xóa bãi đỗ xe. Vui lòng thử lại.');
	}
}

// Thêm hàm tìm kiếm địa điểm
async function searchPlaces(keyword) {
	try {
		const response = await fetch(`${API_URL}/places/search?keyword=${encodeURIComponent(keyword)}`);
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error searching places:', error);
		return null;
	}
}

// Thêm hàm lấy chỉ đường
async function getDirections(fromLat, fromLng, toLat, toLng) {
	try {
		const response = await fetch(`https://rsapi.goong.io/Direction?` + `origin=${fromLat},${fromLng}&` + `destination=${toLat},${toLng}&` + `vehicle=car&` + `alternatives=true&` + `api_key=${GOONG_API_ACCESS}`);
		const data = await response.json();
		return data;
	} catch (error) {
		console.error('Error getting directions:', error);
		return null;
	}
}

// Thêm sự kiện tìm kiếm
document.getElementById('search').addEventListener('input', async (e) => {
	const keyword = e.target.value;
	if (keyword.length < 3) return;

	const results = await searchPlaces(keyword);
	const resultsDiv = document.getElementById('search-results');

	if (results && results.results) {
		resultsDiv.style.display = 'block';
		resultsDiv.innerHTML = results.results
			.map(
				(place) => `
				<div class="search-result-item" 
					 data-lat="${place.geometry.location.lat}"
					 data-lng="${place.geometry.location.lng}">
					${place.name} - ${place.formatted_address}
				</div>
			`
			)
			.join('');
	}
});

// Cập nhật hàm loadParkingLots để thêm nút chỉ đường
function updateParkingLotList(parkingLots) {
	const listElement = document.getElementById('parkingLotList');
	listElement.innerHTML = parkingLots
		.map(
			(lot) => `
			<div class="parking-lot-item">
				<h3>${lot.name}</h3>
				<p>${lot.address}</p>
				<p>Chỗ trống: ${lot.availableSpots}/${lot.capacity}</p>
				<button class="get-directions-btn" 
						onclick="showDirections(${lot.latitude}, ${lot.longitude})">
					Chỉ đường
				</button>
				<button class="delete-btn" onclick="deleteParkingLot(${lot.id})">Xóa</button>
			</div>
		`
		)
		.join('');
}

// Thêm hàm hiển thị chỉ đường
async function showDirections(toLat, toLng) {
	if ('geolocation' in navigator) {
		navigator.geolocation.getCurrentPosition(
			async (position) => {
				const fromLat = position.coords.latitude;
				const fromLng = position.coords.longitude;

				// Xóa đường đi cũ nếu có
				if (directionsLine) {
					directionsLine.remove();
				}

				try {
					const directions = await getDirections(fromLat, fromLng, toLat, toLng);

					if (directions && directions.routes && directions.routes[0]) {
						const route = directions.routes[0];

						// Vẽ đường đi
						const decodedPolyline = decode(route.overview_polyline.points);
						directionsLine = new goongjs.Polyline({
							coordinates: decodedPolyline.map((point) => [point[1], point[0]]),
							strokeColor: '#2196F3',
							strokeWidth: 3,
						}).addTo(map);

						// Điều chỉnh map để hiển thị toàn bộ tuyến đường
						const bounds = new goongjs.LatLngBounds().extend([fromLng, fromLat]).extend([toLng, toLat]);
						map.fitBounds(bounds, { padding: 50 });

						// Hiển thị thông tin chỉ đường
						document.getElementById('directions-panel').style.display = 'block';
						document.getElementById('directions-panel').innerHTML = `
						<div class="route-info">
							<h3>Thông tin chỉ đường</h3>
							<p>Khoảng cách: ${route.legs[0].distance.text}</p>
							<p>Thời gian: ${route.legs[0].duration.text}</p>
							<button onclick="clearDirections()">Xóa chỉ đường</button>
						</div>
					`;
					}
				} catch (error) {
					console.error('Lỗi khi lấy chỉ đường:', error);
					alert('Không thể lấy chỉ đường. Vui lòng thử lại sau.');
				}
			},
			(error) => {
				console.error('Lỗi khi lấy vị trí:', error);
				alert('Không thể lấy vị trí của bạn. Vui lòng kiểm tra quyền truy cập vị trí.');
			}
		);
	} else {
		alert('Trình duyệt của bạn không hỗ trợ Geolocation');
	}
}

// Hàm giải mã polyline từ Google
function decode(str) {
	var points = [];
	var index = 0,
		len = str.length;
	var lat = 0,
		lng = 0;
	while (index < len) {
		var b,
			shift = 0,
			result = 0;
		do {
			b = str.charAt(index++).charCodeAt(0) - 63;
			result |= (b & 0x1f) << shift;
			shift += 5;
		} while (b >= 0x20);
		var dlat = result & 1 ? ~(result >> 1) : result >> 1;
		lat += dlat;
		shift = 0;
		result = 0;
		do {
			b = str.charAt(index++).charCodeAt(0) - 63;
			result |= (b & 0x1f) << shift;
			shift += 5;
		} while (b >= 0x20);
		var dlng = result & 1 ? ~(result >> 1) : result >> 1;
		lng += dlng;
		points.push([lat / 1e5, lng / 1e5]);
	}
	return points;
}

function clearDirections() {
	if (directionsLine) {
		directionsLine.remove();
	}
	document.getElementById('directions-panel').style.display = 'none';
}

// Thêm sự kiện tìm kiếm bãi đỗ xe gần đây
document.getElementById('findNearby').addEventListener('click', async () => {
	if ('geolocation' in navigator) {
		navigator.geolocation.getCurrentPosition(async (position) => {
			const lat = position.coords.latitude;
			const lng = position.coords.longitude;
			const radius = document.getElementById('searchRadius').value;

			const response = await fetch(`${API_URL}/ParkingLots/nearby?lat=${lat}&lng=${lng}&radius=${radius}`);
			const nearbyLots = await response.json();
			updateParkingLotList(nearbyLots);
		});
	}
});

// Load dữ liệu khi trang được tải
loadParkingLots();

// Hàm login
async function login(email, password) {
	try {
		const response = await fetch(`${API_URL}/auth/login`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ email, password }),
		});

		if (response.ok) {
			const data = await response.json();
			accessToken = data.accessToken;
			refreshToken = data.refreshToken;

			// Lưu token vào localStorage
			localStorage.setItem('accessToken', accessToken);
			localStorage.setItem('refreshToken', refreshToken);

			return true;
		}
		return false;
	} catch (error) {
		console.error('Login error:', error);
		return false;
	}
}

// Hàm để thêm authorization header vào mọi request
async function fetchWithAuth(url, options = {}) {
	if (!options.headers) {
		options.headers = {};
	}

	if (accessToken) {
		options.headers['Authorization'] = `Bearer ${accessToken}`;
	}

	try {
		const response = await fetch(url, options);

		if (response.status === 401 && refreshToken) {
			// Thử refresh token
			const newTokens = await refreshAccessToken();
			if (newTokens) {
				// Thử lại request với token mới
				options.headers['Authorization'] = `Bearer ${accessToken}`;
				return fetch(url, options);
			}
		}

		return response;
	} catch (error) {
		console.error('Request error:', error);
		throw error;
	}
}

// Hàm refresh token
async function refreshAccessToken() {
	try {
		const response = await fetch(`${API_URL}/auth/refresh`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ refreshToken }),
		});

		if (response.ok) {
			const data = await response.json();
			accessToken = data.accessToken;
			refreshToken = data.refreshToken;

			localStorage.setItem('accessToken', accessToken);
			localStorage.setItem('refreshToken', refreshToken);

			return true;
		}

		// Nếu refresh thất bại, logout
		logout();
		return false;
	} catch (error) {
		console.error('Refresh token error:', error);
		logout();
		return false;
	}
}

// Hàm logout
function logout() {
	accessToken = null;
	refreshToken = null;
	localStorage.removeItem('accessToken');
	localStorage.removeItem('refreshToken');
	// Redirect về trang login nếu cần
}

// Thêm hàm register
async function register(email, password, firstName, lastName, role) {
	try {
		const response = await fetch(`${API_URL}/auth/register`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				email,
				password,
				firstName,
				lastName,
				role,
			}),
		});

		if (response.ok) {
			const data = await response.json();
			return { success: true, message: data.message };
		}

		const error = await response.json();
		return { success: false, message: error.message };
	} catch (error) {
		console.error('Register error:', error);
		return { success: false, message: 'Đã xảy ra lỗi khi đăng ký' };
	}
}

// Xử lý submit form đăng ký
document.getElementById('registerForm').addEventListener('submit', async (e) => {
	e.preventDefault();

	const email = document.getElementById('registerEmail').value;
	const password = document.getElementById('registerPassword').value;
	const firstName = document.getElementById('firstName').value;
	const lastName = document.getElementById('lastName').value;
	const role = document.getElementById('role').value;

	const result = await register(email, password, firstName, lastName, role);

	if (result.success) {
		alert('Đăng ký thành công! Vui lòng đăng nhập.');
		// Reset form
		e.target.reset();
	} else {
		alert(result.message || 'Đăng ký thất bại');
	}
});
