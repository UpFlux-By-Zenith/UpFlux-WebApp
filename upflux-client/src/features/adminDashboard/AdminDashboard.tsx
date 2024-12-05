import { Footer } from "../footer/Footer";
import { Navbar } from "../navbar/Navbar";
import { GetEngineerToken } from "./getEngineerToken/GetEngineerToken";

export const AdminDashboard = () => {

    return <><Navbar onHomePage={false} />
            <GetEngineerToken/>
        
        <Footer /></>;
}